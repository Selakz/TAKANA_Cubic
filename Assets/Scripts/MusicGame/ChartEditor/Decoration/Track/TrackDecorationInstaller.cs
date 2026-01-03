#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecorationInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			// Track Decorator
			var trackDataset = new HashDataset<ChartComponent>();
			builder.RegisterInstance<IDataset<ChartComponent>>(trackDataset)
				.Keyed("track-decoration");
			builder.RegisterEntryPoint<TrackSourceSystem>();

			// Track's TrackEdgeMovement Decorator
			var edgeMovementDataset = new EdgeDataset(trackDataset, MovementLocator<TrackEdgeMovement>.Factory);
			builder.RegisterInstance(edgeMovementDataset);

			// Track's TrackEdgeMovement's ChartPosMoveList Decorator
			var edgeSideMoveListDataset =
				new EdgePMLDataset(edgeMovementDataset, EdgeSideMovementLocator<ChartPosMoveList>.Factory);
			builder.RegisterInstance(edgeSideMoveListDataset);

			// Track's TrackEdgeMovement's ChartPosMoveList's IPositionMoveItem Decorator (Such a four-layer nesting!!)
			var edgeNodeDataset = new EdgeNodeDataset(edgeSideMoveListDataset, EdgeSideMoveItemLocator.Factory);
			builder.RegisterInstance(edgeNodeDataset);

			builder.Register<EdgeNodeSelectDataset>(Lifetime.Singleton);
		}
	}
}