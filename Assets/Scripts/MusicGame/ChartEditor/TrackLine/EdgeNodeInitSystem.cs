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
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class EdgeNodeInitSystem : HierarchySystem<EdgeNodeInitSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ISpeed>(speed, () =>
			{
				foreach (var component in moveListViewPool) SetNodePosition(component);
			}),
			new DatasetRegistrar<EdgePMLComponent>(moveListViewPool,
				DatasetRegistrar<EdgePMLComponent>.RegisterTarget.DataAdded,
				SetNodePosition),
			new DatasetRegistrar<EdgePMLComponent>(moveListViewPool,
				DatasetRegistrar<EdgePMLComponent>.RegisterTarget.DataUpdated,
				component => UniTask.DelayFrame(1).ContinueWith(() => SetNodePosition(component)))
		};

		// Private
		[Inject] private readonly NotifiableProperty<ISpeed> speed = default!;
		[Inject] private readonly EdgeNodeDataset nodeDataset = default!;
		[Inject] private readonly IViewPool<EdgePMLComponent> moveListViewPool = default!;
		[Inject] private readonly IViewPool<EdgeNodeComponent> nodePool = default!;

		// Defined Functions
		private void SetNodePosition(EdgePMLComponent moveList)
		{
			PrefabHandler? lastHandler = null;
			EdgeNodeComponent? lastNode = null;
			T3Time? lastNodeTime = null;
			var nodes = nodeDataset[moveList].ToArray();
			Array.Sort(nodes, (componentX, componentY) => componentX.Locator.Time.CompareTo(componentY.Locator.Time));
			foreach (var node in nodes)
			{
				var track = (node.Locator.Track.Model as ITrack)!;
				if (lastHandler is not null && lastNode?.Model is not null && lastNodeTime is not null)
				{
					var speedRate = speed.Value.SpeedRate;
					var lastX = lastNode.Model.Position;
					var lastY = (lastNodeTime - track.TimeStart).Value.Second * speedRate;
					var x = node.Model.Position;
					var y = (moveList.Model[node.Model] - track.TimeStart)!.Value.Second * speedRate;
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
				}

				lastHandler = nodePool[node];
				lastNode = node;
				lastNodeTime = moveList.Model[node.Model];
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

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			nodePool.IsGetActive = false;
		}
	}
}