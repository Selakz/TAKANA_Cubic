#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using MusicGame.Gameplay.Stage;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackLineNodeDecoratorSystem : HierarchySystem<TrackLineNodeDecoratorSystem>
	{
		// Serializable and Public
		[SerializeField] private string nodeNamePrefix = "TrackNode";

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler =>
			{
				var track = decoratorPool[handler]!;
				return new CustomRegistrar(
					() =>
					{
						foreach (var node in nodeDataset[track]) TrackNode(node);
					},
					() => UntrackTrackNodes(track));
			}, true),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataAdded,
				node =>
				{
					if (decoratorPool.Contains(node.Track)) TrackNode(node);
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved,
				node =>
				{
					if (trackedNodes.Remove(node)) viewGenerator.Remove(node);
					nodePool.Remove(node);
				}),
			CustomRegistrar.Generic<Action<ChartComponent>>(
				e => nodeDataset.OnTrackSynced += e,
				e => nodeDataset.OnTrackSynced -= e,
				track =>
				{
					if (!decoratorPool.Contains(track)) return;
					foreach (var node in nodeDataset[track])
					{
						if (trackedNodes.Contains(node)) viewGenerator.Update(node);
						else TrackNode(node);
					}
				}),
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				foreach (var node in trackedNodes) viewGenerator.Update(node);
			}),
			new DatasetRegistrar<TrackNodeComponent>(nodePool,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated,
				node =>
				{
					if (nodePool[node] is { } handler &&
					    decoratorPool[node.Track] is { } decorator &&
					    handler.Parent != decorator)
					{
						decorator.AddPlugin(GetPluginId(node), handler);
					}
				}),
			new ViewPoolRegistrar<TrackNodeComponent>(nodePool,
				ViewPoolRegistrar<TrackNodeComponent>.RegisterTarget.Release,
				handler => handler.Parent?.RemovePlugin(handler, nodePool.DefaultTransform))
		};

		// Private
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private readonly TrackNodeDataset nodeDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;
		[Inject] private readonly IGameAudioPlayer music = default!;
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;

		private readonly HashSet<TrackNodeComponent> trackedNodes = new();
		private TimeWindowViewGenerator<TrackNodeComponent> viewGenerator = default!;

		// Defined Functions
		private void TrackNode(TrackNodeComponent node)
		{
			if (trackedNodes.Add(node)) viewGenerator.Add(node);
		}

		private void UntrackTrackNodes(ChartComponent track)
		{
			List<TrackNodeComponent> toRemove = new();
			foreach (var node in trackedNodes)
			{
				if (node.Track == track) toRemove.Add(node);
			}

			foreach (var node in toRemove)
			{
				trackedNodes.Remove(node);
				viewGenerator.Remove(node);
				nodePool.Remove(node);
			}
		}

		private string GetPluginId(TrackNodeComponent node) =>
			$"{nodeNamePrefix}_{(node.IsFirst ? 1 : 2)}_{node.GetHashCode()}";

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			viewGenerator = new(new TrackNodeTimeCalculator(speed));
		}

		void Update()
		{
			viewGenerator.RefreshTime(music.ChartTime, out var toInstantiate, out var toDestroy);
			foreach (var node in toDestroy)
			{
				nodePool.Remove(node);
			}

			foreach (var node in toInstantiate)
			{
				if (decoratorPool[node.Track] is not { } decorator) continue;
				if (nodePool.Add(node))
				{
					decorator.AddPlugin(GetPluginId(node), nodePool[node]!);
				}
			}
		}

		// Inner Classes
		private class TrackNodeTimeCalculator : ITimeCalculator<TrackNodeComponent>
		{
			private readonly NotifiableProperty<ISpeed> speed;

			public TrackNodeTimeCalculator(NotifiableProperty<ISpeed> speed) => this.speed = speed;

			public T3Time GetTimeInstantiate(TrackNodeComponent? item)
			{
				if (item is null) return T3Time.MinValue;
				var speedRate = Mathf.Max(speed.Value.SpeedRate, 0.01f);
				return item.Time.Value - new T3Time(ISingleton<PlayfieldSetting>.Instance.UpperThreshold.Value / speedRate);
			}

			public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

			public T3Time GetTimeDestroy(TrackNodeComponent? item)
			{
				if (item is null) return T3Time.MaxValue;
				var endTime = item.Time.Value;
				if (item.IsPosition && item.MoveList is { } moveList)
				{
					var index = moveList.BinarySearch(item.Time.Value);
					if (index >= 0 && index + 1 < moveList.Count)
					{
						endTime = moveList[index + 1].Key;
					}
				}

				var speedRate = Mathf.Max(speed.Value.SpeedRate, 0.01f);
				var lowerTime = endTime - new T3Time(ISingleton<PlayfieldSetting>.Instance.LowerThreshold.Value / speedRate);
				var afterEndTime = endTime + new T3Time(ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd.Value);
				return lowerTime > afterEndTime ? lowerTime : afterEndTime;
			}

			public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime) => selfTime;

			public TrackNodeComponent? GetParent(TrackNodeComponent item) => null;

			public IEnumerable<TrackNodeComponent> GetChildren(TrackNodeComponent item) =>
				Enumerable.Empty<TrackNodeComponent>();
		}
	}
}
