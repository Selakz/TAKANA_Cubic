#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecorationViewScope : HierarchyLifetimeScope
	{
		[SerializeField] private ViewPoolInstaller decoratorPool = default!;
		[SerializeField] private ViewPoolInstaller edgeMovementPool = default!;
		[SerializeField] private ViewPoolInstaller edgeSideMoveListPool = default!;
		[Header("Edge Node")] [SerializeField] private string nodeNamePrefix = string.Empty;
		[SerializeField] private ViewPoolInstaller edgeNodePool = default!;

		protected override void Configure(IContainerBuilder builder)
		{
			base.Configure(builder);

			// Track Decorator
			decoratorPool.Register<ViewPool<ChartComponent>, ChartComponent>(builder, Lifetime.Singleton)
				.Keyed("track-decoration");
			builder.RegisterEntryPoint<TrackDecoratorSystem>();

			// Track's TrackEdgeMovement Decorator
			edgeMovementPool.Register<ViewPool<EdgeComponent>, EdgeComponent>(builder, Lifetime.Singleton);
			builder.RegisterEntryPoint<EdgeDecoratorSystem>();

			// Track's TrackEdgeMovement's ChartPosMoveList Decorator
			edgeSideMoveListPool.Register<ViewPool<EdgePMLComponent>, EdgePMLComponent>(builder, Lifetime.Singleton);
			builder.RegisterEntryPoint<EdgeSideDecoratorSystem>();

			// Track's TrackEdgeMovement's ChartPosMoveList's IPositionMoveItem Decorator (Such a four-layer nesting!!)
			edgeNodePool.Register<ViewPool<EdgeNodeComponent>, EdgeNodeComponent>(builder, Lifetime.Singleton);
			builder.RegisterEntryPoint<EdgeNodeDecoratorSystem>()
				.WithParameter("nodeNamePrefix", nodeNamePrefix);
		}
	}
}