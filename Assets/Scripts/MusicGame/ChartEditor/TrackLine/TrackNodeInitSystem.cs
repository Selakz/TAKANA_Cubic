#nullable enable

using System;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine.Render;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackNodeInitSystem : HierarchySystem<TrackNodeInitSystem>
	{
		// Serializable and Public
		[SerializeField] private int maxTimePluginShouldInit = 60000;
		[SerializeField] private ViewPoolInstaller edgePluginInstaller;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<TrackNodeComponent>(nodePool, edgePluginPool, "edge-plugin",
				shouldAddPlugin: node => node.Type == TrackNodeType.Pos),
			new ViewPoolRegistrar<TrackNodeComponent>(nodePool,
				ViewPoolRegistrar<TrackNodeComponent>.RegisterTarget.Get,
				handler =>
				{
					if (nodePool[handler] is { } node) InitNode(node);
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodePool,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated,
				InitNode),
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				foreach (var node in nodePool) InitNode(node);
			}),
			CustomRegistrar.Generic<Action<ChartComponent>>(
				e => nodeDataset.OnTrackSynced += e,
				e => nodeDataset.OnTrackSynced -= e,
				track =>
				{
					foreach (var node in nodePool)
					{
						if (node.Track == track) InitNode(node);
					}
				})
		};

		// Private
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;
		[Inject] private readonly TrackNodeDataset nodeDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;
		[Inject] [Key("edge-plugin")] private readonly IViewPool<TrackNodeComponent> edgePluginPool = default!;

		// Constructor
		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<TrackNodeComponent>(edgePluginInstaller).Keyed("edge-plugin");
		}

		// Defined Functions
		private void InitNode(TrackNodeComponent node)
		{
			if (nodePool[node] is not { } handler || node.Track.Model is not ITrack trackModel) return;
			var speedRate = speed.Value.SpeedRate;
			if (node.Type == TrackNodeType.Width)
			{
				var view = handler.Script<WidthNodeView>();
				var x = trackModel.Movement.GetPos(node.Time.Value);
				var y = (node.Time.Value - trackModel.TimeStart).Second * speedRate;
				switch (node.Model)
				{
					case V1EMoveItem easeItem:
						view.Init(easeItem.Ease.GetString(), new(x, y), easeItem.Position);
						break;
					case V1BMoveItem bezierItem:
						view.Init("bezier", new(x, y), bezierItem.Position);
						break;
				}

				return;
			}

			var currentTime = node.Time.Value;
			var currentPos = new Vector2(node.Model.Position, (currentTime - trackModel.TimeStart).Second * speedRate);
			if (node.MoveList is { } moveList)
			{
				var index = moveList.BinarySearch(currentTime);
				if (index >= 0 && index + 1 < moveList.Count)
				{
					var nextPair = moveList[index + 1];
					var nextTime = nextPair.Key;
					var nextPos = new Vector2(
						nextPair.Value.Position, (nextTime - trackModel.TimeStart).Second * speedRate);
					switch (node.Model)
					{
						case V1EMoveItem easeItem:
							handler.Script<EaseLineRenderer>().Init(easeItem.Ease, currentPos, nextPos);
							break;
						case V1BMoveItem bezierItem:
							handler.Script<BezierLineRenderer>().Init(
								bezierItem.StartControlFactor, bezierItem.EndControlFactor, currentPos, nextPos);
							break;
					}

					if (node.Type == TrackNodeType.Pos &&
					    handler.GetPlugin("edge-plugin")?.Script<PositionNodePlugin>() is { } plugin)
					{
						if (nextTime - currentTime > maxTimePluginShouldInit)
						{
							plugin.gameObject.SetActive(false);
						}
						else
						{
							plugin.gameObject.SetActive(true);
							SetPluginData(plugin, trackModel, currentTime, nextTime, true);
							SetPluginData(plugin, trackModel, currentTime, nextTime, false);
						}
					}

					return;
				}
			}

			switch (node.Model)
			{
				case V1EMoveItem:
					handler.Script<EaseLineRenderer>().Init(Eases.Unmove, currentPos, currentPos);
					break;
				case V1BMoveItem bezierItem:
					handler.Script<BezierLineRenderer>().Init(
						bezierItem.StartControlFactor, bezierItem.EndControlFactor, currentPos, currentPos);
					break;
			}

			if (handler.GetPlugin("edge-plugin") is { } tailPlugin)
			{
				tailPlugin.gameObject.SetActive(false);
			}

			return;

			void SetPluginData(PositionNodePlugin plugin, ITrack track, T3Time timeStart, T3Time timeEnd, bool isLeft)
			{
				var step = (timeEnd - timeStart).Second / plugin.SegmentCount;
				Func<T3Time, float> getPosFunc = isLeft ? track.Movement.GetLeftPos : track.Movement.GetRightPos;
				var start = getPosFunc.Invoke(timeStart);
				var end = getPosFunc.Invoke(timeEnd);
				Vector4 data = new(0, 0);
				for (var i = 1; i < plugin.SegmentCount; i++)
				{
					var time = timeStart + step * i;
					var pos = Mathf.Approximately(end, start) ? 0 : (getPosFunc.Invoke(time) - start) / (end - start);
					var t = (time - timeStart).Second / (timeEnd - timeStart).Second;
					if (i % 2 == 0)
					{
						data.x = t;
						data.y = pos;
					}
					else
					{
						data.z = t;
						data.w = pos;
						var segments = isLeft ? plugin.LeftSegments : plugin.RightSegments;
						segments[i / 2] = data;
					}
				}

				var basePos = track.Movement.GetPos(timeStart);
				var y = (timeEnd - timeStart).Second * speed.Value.SpeedRate;
				var r = isLeft ? plugin.LeftRenderer : plugin.RightRenderer;
				r.Init(isLeft ? plugin.LeftSegments : plugin.RightSegments,
					new(start - basePos, 0), new(end - basePos, y));
			}
		}

		// System Functions
		protected override void OnEnable()
		{
			nodePool.IsGetActive = false;
			base.OnEnable();
		}
	}
}
