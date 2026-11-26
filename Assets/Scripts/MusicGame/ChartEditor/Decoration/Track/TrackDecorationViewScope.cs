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
		[SerializeField] private ClassViewPoolInstaller<NodeClassifyType> trackNodePool = default!;

		protected override void Configure(IContainerBuilder builder)
		{
			base.Configure(builder);

			// Track Decorator
			decoratorPool.Register<ViewPool<ChartComponent>, ChartComponent>(builder, Lifetime.Singleton)
				.Keyed("track-decoration");

			// Track Node
			trackNodePool.Register<ViewPool<TrackNodeComponent, NodeClassifyType>, TrackNodeComponent>(
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
				TrackNodeComponent trackNode => trackNode.Model switch
				{
					V1EMoveItem when trackNode.IsPosition => NodeClassifyType.Ease | NodeClassifyType.Position,
					V1EMoveItem => NodeClassifyType.Ease | NodeClassifyType.Width,
					V1BMoveItem when trackNode.IsPosition => NodeClassifyType.Bezier | NodeClassifyType.Position,
					V1BMoveItem => NodeClassifyType.Bezier | NodeClassifyType.Width,
					_ => NodeClassifyType.None
				},
				_ => NodeClassifyType.None
			};
		}

		public bool IsOfType(IComponent component, NodeClassifyType type) => Classify(component).HasFlag(type);

		public bool IsSubType(NodeClassifyType subType, NodeClassifyType type) => type.HasFlag(subType);
	}
}
