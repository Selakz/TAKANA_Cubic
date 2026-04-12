#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecorationViewScope : HierarchyLifetimeScope
	{
		[SerializeField] private ViewPoolInstaller decoratorPool = default!;

		[Header("Edge Movement")] [SerializeField]
		private ViewPoolInstaller edgeMovementPool = default!;

		[SerializeField] private ViewPoolInstaller edgeSideMoveListPool = default!;
		[SerializeField] private ClassViewPoolInstaller<NodeClassifyType> edgeNodePool = default!;

		[Header("Direct Movement")] [SerializeField]
		private ViewPoolInstaller directMovementPool = default!;

		[SerializeField] private ViewPoolInstaller directSideMoveListPool = default!;
		[SerializeField] private ClassViewPoolInstaller<NodeClassifyType> directNodePool = default!;

		protected override void Configure(IContainerBuilder builder)
		{
			base.Configure(builder);

			// Track Decorator
			decoratorPool.Register<ViewPool<ChartComponent>, ChartComponent>(builder, Lifetime.Singleton)
				.Keyed("track-decoration");

			// Track's TrackEdgeMovement Decorator
			edgeMovementPool.Register<ViewPool<EdgeComponent>, EdgeComponent>(
				builder, Lifetime.Singleton);

			// Track's TrackEdgeMovement's ChartPosMoveList Decorator
			edgeSideMoveListPool.Register<ViewPool<EdgePMLComponent>, EdgePMLComponent>(
				builder, Lifetime.Singleton);

			// Track's TrackEdgeMovement's ChartPosMoveList's IPositionMoveItem Decorator (Such a four-layer nesting!!)
			edgeNodePool.Register<ViewPool<EdgeNodeComponent, NodeClassifyType>, EdgeNodeComponent>(
				builder, Lifetime.Singleton, new NodeClassifier());

			// Track's TrackDirectMovement Decorator
			directMovementPool.Register<ViewPool<DirectComponent>, DirectComponent>(
				builder, Lifetime.Singleton);

			// Track's TrackDirectMovement's ChartPosMoveList Decorator
			directSideMoveListPool.Register<ViewPool<DirectPMLComponent>, DirectPMLComponent>(
				builder, Lifetime.Singleton);

			// Track's TrackDirectMovement's ChartPosMoveList's IPositionMoveItem Decorator (Again a four-layer nesting!!)
			directNodePool.Register<ViewPool<DirectNodeComponent, NodeClassifyType>, DirectNodeComponent>(
				builder, Lifetime.Singleton, NodeClassifier.Instance);
		}
	}

	[Flags]
	public enum NodeClassifyType
	{
		None = 0,
		Ease = 1,
		Bezier = 1 << 1,
		Width = 1 << 2,
		Position = 1 << 3
	}

	public class NodeClassifier : IClassifier<NodeClassifyType>
	{
		public static NodeClassifier Instance { get; } = new();

		public NodeClassifyType Classify(IComponent component)
		{
			return component switch
			{
				EdgeNodeComponent edgeNode => edgeNode.Model switch
				{
					V1EMoveItem => NodeClassifyType.Ease | NodeClassifyType.Position,
					V1BMoveItem => NodeClassifyType.Bezier | NodeClassifyType.Position,
					_ => NodeClassifyType.None
				},
				DirectNodeComponent directNode => directNode.Model switch
				{
					V1EMoveItem when directNode.Locator.IsPos => NodeClassifyType.Ease | NodeClassifyType.Position,
					V1EMoveItem when !directNode.Locator.IsPos => NodeClassifyType.Ease | NodeClassifyType.Width,
					V1BMoveItem when directNode.Locator.IsPos => NodeClassifyType.Bezier | NodeClassifyType.Position,
					V1BMoveItem when !directNode.Locator.IsPos => NodeClassifyType.Bezier | NodeClassifyType.Width,
					_ => NodeClassifyType.None
				},
				_ => NodeClassifyType.None
			};
		}

		public bool IsOfType(IComponent component, NodeClassifyType type) => Classify(component).HasFlag(type);

		public bool IsSubType(NodeClassifyType subType, NodeClassifyType type) => type.HasFlag(subType);
	}
}