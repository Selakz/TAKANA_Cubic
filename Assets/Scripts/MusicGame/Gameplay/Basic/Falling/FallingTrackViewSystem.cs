#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.TrackLine.Render;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using MusicGame.Gameplay.Stage;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.Falling
{
	public class TrackChunkData
	{
		public ChartComponent Track { get; }

		public int Index { get; }

		public VectorArrayConverter LeftSegments { get; }

		public VectorArrayConverter RightSegments { get; }

		public TrackChunkData(ChartComponent track, int index, int segmentCount)
		{
			Track = track;
			Index = index;
			LeftSegments = new(segmentCount);
			RightSegments = new(segmentCount);
		}
	}

	public class TrackChunkTimeCalculator : ITimeCalculator<TrackChunkData>
	{
		private readonly T3Time chunkTime;

		public TrackChunkTimeCalculator(T3Time chunkTime)
		{
			this.chunkTime = chunkTime;
		}

		public T3Time GetTimeInstantiate(TrackChunkData? item)
		{
			if (item is null) return T3Time.MinValue;
			return item.Track.Model.TimeMin - ISingleton<PlayfieldSetting>.Instance.UpperThreshold +
			       item.Index * chunkTime.Second;
		}

		public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

		public T3Time GetTimeDestroy(TrackChunkData? item)
		{
			if (item is null) return T3Time.MaxValue;
			return item.Track.Model.TimeMin + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd +
			       (item.Index + 1) * chunkTime.Second;
		}

		public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

		public TrackChunkData? GetParent(TrackChunkData item) => null;

		public IEnumerable<TrackChunkData> GetChildren(TrackChunkData item) => Enumerable.Empty<TrackChunkData>();
	}

	public class FallingTrackViewSystem : HierarchySystem<FallingTrackViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private PrefabObject trackChunkPrefab = default!;
		[SerializeField] private int chunkTimeMs = 2000;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<GameplayStageSkinConfig>(service.OnStageReset,
				config => IsEnabled = config.trackBehaviour is TrackBehaviour.Falling)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var component = viewPool[handler]!;
					if (component.Model is not ITrack track) return;

					// Update position
					var presenter = handler.Script<FallingTrackViewPresenter>();
					presenter.PositionModifier.Register(
						value => new(value.x, (track.TimeStart - music.ChartTime) * speed.Value.SpeedRate),
						positionPriority);
					UpdateStartEndSprite(track, presenter);

					// Update chunks
					var chunkTime = new T3Time(chunkTimeMs);
					var trackDuration = (track.TimeEnd - track.TimeStart).Second;
					int chunkCount = Mathf.CeilToInt(trackDuration / chunkTime.Second);
					for (int i = 0; i < chunkCount; i++)
					{
						var data = new TrackChunkData(component, i, SegmentCount);
						var timeStart = track.TimeStart + chunkTime.Second * i;
						var timeEnd = Mathf.Min(track.TimeEnd, track.TimeStart + chunkTime.Second * (i + 1));
						FillSegments(track, timeStart, timeEnd, data.LeftSegments, data.RightSegments);
						currentChunks.Add(data);
						chunkViewGenerator.Add(data);
					}
				},
				() =>
				{
					var component = viewPool[handler]!;
					if (component.Model is not ITrack) return;

					// Clear position
					var presenter = handler.Script<FallingTrackViewPresenter>();
					presenter.PositionModifier.Unregister(positionPriority, true);
					presenter.StartLineModifier.SizeModifier.Unregister(0, true);

					// Clear chunks
					presenter.ClearAllChunks(); // insurance
					var toRemove = currentChunks.Where(c => c.Track == component).ToList();
					foreach (var data in toRemove)
					{
						chunkViewGenerator.Remove(data);
						currentChunks.Remove(data);
					}
				}), true),
			new DatasetRegistrar<ChartComponent>(viewPool, DatasetRegistrar<ChartComponent>.RegisterTarget.DataUpdated,
				component =>
				{
					if (component.Model is not ITrack track) return;
					var chunkTime = new T3Time(chunkTimeMs);
					var dataList = currentChunks.Where(c => c.Track == component).ToList();

					foreach (var data in dataList)
					{
						var timeStart = track.TimeStart + chunkTime.Second * data.Index;
						var timeEnd = Mathf.Min(track.TimeEnd, track.TimeStart + chunkTime.Second * (data.Index + 1));
						FillSegments(track, timeStart, timeEnd, data.LeftSegments, data.RightSegments);
						chunkViewGenerator.Update(data);
					}

					if (viewPool[component] is { } handler)
					{
						var presenter = handler.Script<FallingTrackViewPresenter>();
						presenter.UpdateChunks((i, chunk) =>
						{
							var data = dataList.FirstOrDefault(d => d.Index == i);
							if (data is not null) InitChunk(chunk, data);
						});
						UpdateStartEndSprite(track, presenter);
					}
				}),
			new PropertyRegistrar<ISpeed>(speed,
				() =>
				{
					foreach (var component in viewPool)
					{
						if (component.Model is not ITrack track) continue;
						var handler = viewPool[component]!;
						var presenter = handler.Script<FallingTrackViewPresenter>();
						presenter.UpdateChunks((i, chunk) =>
						{
							var data = currentChunks.FirstOrDefault(d => d.Track == component && d.Index == i);
							if (data is null) return;
							InitChunk(chunk, data);
						});
						UpdateStartEndSprite(track, presenter);
					}
				})
		};

		// Private
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
		[Inject] private IStageViewGenerateService service = default!;
		[Inject] private NotifiableProperty<ISpeed> speed = default!;
		[Inject] private IGameAudioPlayer music = default!;

		private TimeWindowViewGenerator<TrackChunkData> chunkViewGenerator = default!;
		private readonly List<TrackChunkData> currentChunks = new();

		// Static
		private const int SegmentCount = 100;

		// Defined Functions
		private static void FillSegments(ITrack track, T3Time timeStart, T3Time timeEnd,
			VectorArrayConverter leftSegments, VectorArrayConverter rightSegments)
		{
			int count = leftSegments.Length;
			var step = (timeEnd - timeStart).Second / count;
			float leftStart = track.Movement.GetLeftPos(timeStart);
			float leftEnd = track.Movement.GetLeftPos(timeEnd);
			float rightStart = track.Movement.GetRightPos(timeStart);
			float rightEnd = track.Movement.GetRightPos(timeEnd);

			for (int i = 0; i < count; i++)
			{
				var time = timeStart + step * i;
				float t = (time - timeStart).Second / (timeEnd - timeStart).Second;

				float lPos = Mathf.Approximately(leftEnd, leftStart)
					? 0
					: (track.Movement.GetLeftPos(time) - leftStart) / (leftEnd - leftStart);
				float rPos = Mathf.Approximately(rightEnd, rightStart)
					? 0
					: (track.Movement.GetRightPos(time) - rightStart) / (rightEnd - rightStart);

				leftSegments[i] = new Vector2(t, lPos);
				rightSegments[i] = new Vector2(t, rPos);
			}
		}

		private void InitChunk(TrackChunk chunk, TrackChunkData data)
		{
			if (data.Track.Model is not ITrack track) return;
			var chunkTime = new T3Time(chunkTimeMs);
			var timeStart = track.TimeStart + chunkTime.Second * data.Index;
			var timeEnd = Mathf.Min(track.TimeEnd, track.TimeStart + chunkTime.Second * (data.Index + 1));
			var speedRate = speed.Value.SpeedRate;
			float leftStart = track.Movement.GetLeftPos(timeStart);
			float leftEnd = track.Movement.GetLeftPos(timeEnd);
			float rightStart = track.Movement.GetRightPos(timeStart);
			float rightEnd = track.Movement.GetRightPos(timeEnd);
			float indexHeight = chunkTime.Second * speedRate;
			float y = (timeEnd - timeStart).Second * speedRate;

			chunk.LeftLine.Init(data.LeftSegments.AsVector4Array(),
				new(leftStart, 0), new(leftEnd, y));
			chunk.RightLine.Init(data.RightSegments.AsVector4Array(),
				new(rightStart, 0), new(rightEnd, y));
			chunk.Plane.Init(data.LeftSegments.AsVector4Array(), data.RightSegments.AsVector4Array(),
				y, leftEnd - leftStart, leftStart,
				y, rightEnd - rightStart, rightStart);
			chunk.transform.localPosition = new Vector3(0, indexHeight * data.Index, 0);
		}

		private static void UpdateStartEndSprite(ITrack track, FallingTrackViewPresenter presenter)
		{
			var startWidth = track.Movement.GetWidth(track.TimeStart);
			var startPos = track.Movement.GetPos(track.TimeStart);
			presenter.StartLineModifier.Value.transform.localPosition = new(startPos, 0);
			presenter.StartLineModifier.SizeModifier.Register(value => new(startWidth, value.y), 0);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			chunkViewGenerator = new(new TrackChunkTimeCalculator(chunkTimeMs));
			ISingleton<ChunkSpritePoolManager>.Instance.ChunkPrefab = trackChunkPrefab;
		}

		void Update()
		{
			chunkViewGenerator.RefreshTime(music.ChartTime, out var toInstantiate, out var toDestroy);
			foreach (var data in toInstantiate)
			{
				var handler = viewPool[data.Track]!;
				var presenter = handler.Script<FallingTrackViewPresenter>();
				presenter.AddChunk(data.Index, (_, chunk) => InitChunk(chunk, data));
			}

			foreach (var data in toDestroy)
			{
				if (viewPool[data.Track] is { } handler)
				{
					var presenter = handler.Script<FallingTrackViewPresenter>();
					presenter.RemoveChunk(data.Index);
				}
			}
		}
	}
}