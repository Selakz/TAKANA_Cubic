#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.TrackLine.Render;
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
	public class DirectNodeInitSystem : HierarchySystem<DirectNodeInitSystem>
	{
		// Serializable and Public
		[SerializeField] private int maxTimePluginShouldInit = 60000;
		[SerializeField] private ViewPoolInstaller edgePluginInstaller;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				foreach (var component in moveListViewPool) SetNodePosition(component);
			}),
			new DatasetRegistrar<DirectPMLComponent>(moveListViewPool,
				DatasetRegistrar<DirectPMLComponent>.RegisterTarget.DataAdded,
				SetNodePosition),
			new DatasetRegistrar<DirectPMLComponent>(moveListViewPool,
				DatasetRegistrar<DirectPMLComponent>.RegisterTarget.DataUpdated,
				component => UniTask.DelayFrame(1).ContinueWith(() => SetNodePosition(component))),
			new ViewPoolPluginRegistrar<DirectNodeComponent>(nodePool, edgePluginPool, "edge-plugin",
				shouldAddPlugin: node => node.Locator.IsPos)
		};

		// Private
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;
		[Inject] private readonly DirectNodeDataset nodeDataset = default!;
		[Inject] private readonly IViewPool<DirectPMLComponent> moveListViewPool = default!;
		[Inject] private readonly IViewPool<DirectNodeComponent> nodePool = default!;
		[Inject] [Key("edge-plugin")] private readonly IViewPool<DirectNodeComponent> edgePluginPool = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<DirectNodeComponent>(edgePluginInstaller).Keyed("edge-plugin");
		}

		// Defined Functions
		private void SetNodePosition(DirectPMLComponent moveList)
		{
			PrefabHandler? lastHandler = null;
			DirectNodeComponent? lastNode = null;
			T3Time? lastNodeTime = null;
			var nodes = nodeDataset[moveList].ToArray();
			Array.Sort(nodes, (componentX, componentY) => componentX.Locator.Time.CompareTo(componentY.Locator.Time));
			foreach (var node in nodes)
			{
				var track = (node.Locator.Track.Model as ITrack)!;
				if (!node.Locator.IsPos)
				{
					var view = nodePool[node]!.Script<WidthNodeView>();
					var x = track.Movement.GetPos(node.Locator.Time);
					var y = (node.Locator.Time - track.TimeStart).Second * speed.Value.SpeedRate;
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
					if (lastHandler is not null && lastNode is not null && lastNodeTime is not null)
					{
						var speedRate = speed.Value.SpeedRate;
						var lastX = lastNode.Model.Position;
						var lastY = (lastNodeTime - track.TimeStart).Value.Second * speedRate;
						var x = node.Model.Position;
						var y = (node.Locator.Time - track.TimeStart).Second * speedRate;
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

						if (lastHandler.GetPlugin("edge-plugin")?.Script<PositionNodePlugin>() is { } plugin)
						{
							if (node.Locator.Time - lastNodeTime.Value > maxTimePluginShouldInit)
							{
								plugin.gameObject.SetActive(false);
							}
							else
							{
								plugin.gameObject.SetActive(true);
								SetPluginData(plugin, track, lastNodeTime.Value, node.Locator.Time, true);
								SetPluginData(plugin, track, lastNodeTime.Value, node.Locator.Time, false);
							}
						}
					}

					lastHandler = nodePool[node];
					lastNode = node;
					lastNodeTime = node.Locator.Time;
				}
			}

			if (lastHandler is not null && lastNode is not null && lastNodeTime is not null)
			{
				var track = (lastNode.Locator.Track.Model as ITrack)!;
				var easeRenderer = lastHandler.Script<EaseLineRenderer>();
				var lastX = lastNode.Model.Position;
				var lastY = (lastNodeTime - track.TimeStart).Value.Second * speed.Value.SpeedRate;
				easeRenderer.Init(Eases.Unmove, new(lastX, lastY), new(lastX, lastY));
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