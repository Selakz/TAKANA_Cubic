#nullable enable

using System;
using System.Linq;
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
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				foreach (var track in decoratorPool) InitTrackNodes(track);
			}),
			new DatasetRegistrar<ChartComponent>(decoratorPool,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded,
				InitTrackNodes),
			CustomRegistrar.Generic<Action<ChartComponent>>(
				e => nodeDataset.OnTrackSynced += e,
				e => nodeDataset.OnTrackSynced -= e,
				InitTrackNodes),
			new ViewPoolPluginRegistrar<TrackNodeComponent>(nodePool, edgePluginPool, "edge-plugin",
				shouldAddPlugin: node => node.Type == TrackNodeType.Pos)
		};

		// Private
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
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
		private void InitTrackNodes(ChartComponent track)
		{
			if (decoratorPool[track] is null || track.Model is not ITrack trackModel) return;
			InitSideNodes(true);
			InitSideNodes(false);
			return;

			void InitSideNodes(bool isFirst)
			{
				PrefabHandler? lastHandler = null;
				TrackNodeComponent? lastNode = null;
				var nodes = nodeDataset.GetNodes(track, isFirst).ToArray();
				Array.Sort(nodes, (a, b) => a.Time.Value.CompareTo(b.Time.Value));

				var speedRate = speed.Value.SpeedRate;
				foreach (var node in nodes)
				{
					if (nodePool[node] is not { } handler) continue;
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
					}
					else
					{
						if (lastHandler is not null && lastNode is not null)
						{
							var lastTime = lastNode.Time.Value;
							var currentTime = node.Time.Value;
							var lastX = lastNode.Model.Position;
							var lastY = (lastTime - trackModel.TimeStart).Second * speedRate;
							var x = node.Model.Position;
							var y = (currentTime - trackModel.TimeStart).Second * speedRate;
							switch (lastNode.Model)
							{
								case V1EMoveItem easeItem:
									var easeRenderer = lastHandler.Script<EaseLineRenderer>();
									easeRenderer.Init(easeItem.Ease, new(lastX, lastY), new(x, y));
									break;
								case V1BMoveItem bezierItem:
									var bezierRenderer = lastHandler.Script<BezierLineRenderer>();
									bezierRenderer.Init(bezierItem.StartControlFactor, bezierItem.EndControlFactor,
										new(lastX, lastY), new(x, y));
									break;
							}

							if (lastNode.Type == TrackNodeType.Pos &&
							    lastHandler.GetPlugin("edge-plugin")?.Script<PositionNodePlugin>() is { } plugin)
							{
								if (currentTime - lastTime > maxTimePluginShouldInit)
								{
									plugin.gameObject.SetActive(false);
								}
								else
								{
									plugin.gameObject.SetActive(true);
									SetPluginData(plugin, trackModel, lastTime, currentTime, true);
									SetPluginData(plugin, trackModel, lastTime, currentTime, false);
								}
							}
						}

						lastHandler = handler;
						lastNode = node;
					}
				}

				if (lastHandler is not null && lastNode is not null)
				{
					var easeRenderer = lastHandler.Script<EaseLineRenderer>();
					var lastX = lastNode.Model.Position;
					var lastY = (lastNode.Time.Value - trackModel.TimeStart).Second * speedRate;
					easeRenderer.Init(Eases.Unmove, new(lastX, lastY), new(lastX, lastY));
					if (lastHandler.GetPlugin("edge-plugin") is { } tailPlugin)
					{
						tailPlugin.gameObject.SetActive(false);
					}
				}
			}
		}

		private void SetPluginData(
			PositionNodePlugin plugin, ITrack track, T3Time timeStart, T3Time timeEnd, bool isLeft)
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

		// System Functions
		protected override void OnEnable()
		{
			nodePool.IsGetActive = false;
			base.OnEnable();
		}
	}
}
