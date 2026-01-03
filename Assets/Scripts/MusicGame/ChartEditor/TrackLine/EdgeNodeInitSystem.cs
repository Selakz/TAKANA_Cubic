#nullable enable

using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.TrackLine
{
	public class EdgeNodeInitSystem : T3System
	{
		private readonly NotifiableProperty<ISpeed> speed;
		private readonly EdgeNodeDataset nodeDataset;
		private readonly IViewPool<EdgePMLComponent> moveListViewPool;
		private readonly IViewPool<EdgeNodeComponent> nodePool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
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

		public EdgeNodeInitSystem(
			NotifiableProperty<ISpeed> speed,
			EdgeNodeDataset nodeDataset,
			IViewPool<EdgePMLComponent> moveListViewPool,
			IViewPool<EdgeNodeComponent> nodePool) : base(true)
		{
			this.speed = speed;
			this.nodeDataset = nodeDataset;
			this.moveListViewPool = moveListViewPool;
			this.nodePool = nodePool;
			nodePool.IsGetActive = false;
		}

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
				if (lastHandler is not null && lastNode?.Model is V1EMoveItem item && lastNodeTime is not null)
				{
					var nodeView = lastHandler.Script<PositionNodeView>();
					var lastX = lastNode.Model.Position;
					var lastY = (lastNodeTime - track.TimeStart).Value.Second * speed.Value.SpeedRate;
					var x = node.Model.Position;
					var y = (moveList.Model[node.Model] - track.TimeStart)!.Value.Second * speed.Value.SpeedRate;
					nodeView.Init(item.Ease, new(lastX, lastY), new(x, y));
				}

				lastHandler = nodePool[node];
				lastNode = node;
				lastNodeTime = moveList.Model[node.Model];
			}

			if (lastHandler is not null && lastNode is not null && lastNodeTime is not null)
			{
				var track = (lastNode.Locator.Track.Model as ITrack)!;
				var nodeView = lastHandler.Script<PositionNodeView>();
				var lastX = lastNode.Model.Position;
				var lastY = (lastNodeTime - track.TimeStart).Value.Second * speed.Value.SpeedRate;
				nodeView.Init(Eases.Unmove, new(lastX, lastY), new(lastX, lastY));
			}
		}
	}
}