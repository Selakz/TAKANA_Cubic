#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;
using T3Framework.Static.Movement;

namespace MusicGame.ChartEditor.TrackLine
{
	public enum NodeType
	{
		Left,
		Right,
		Pos,
		Width
	}

	public class NodeRawInfo : IComponent
	{
		public NotifiableProperty<T3Time> Time { get; }

		public NotifiableProperty<NodeType> Type { get; }

		public NotifiableProperty<IPositionMoveItem<float>> Node { get; }

		public NotifiableProperty<ChartComponent> Parent { get; }

		public NodeRawInfo(T3Time time, NodeType type, IPositionMoveItem<float> node, ChartComponent parent)
		{
			Time = new(time);
			Type = new(type);
			Node = new(node);
			Parent = new(parent);
			Time.PropertyChanged += (_, _) => UpdateNotify();
			Type.PropertyChanged += (_, _) => UpdateNotify();
			Node.PropertyChanged += (_, _) => UpdateNotify();
			Parent.PropertyChanged += (_, _) => UpdateNotify();
		}

		public NodeRawInfo Clone() => new(Time, Type, Node.Value.Clone(), Parent);

		public static NodeRawInfo FromComponent(IComponent<IPositionMoveItem<float>> component)
		{
			var clonedNode = component.Model.Clone();
			switch (component)
			{
				case EdgeNodeComponent edge:
					var type = edge.Locator.IsLeft ? NodeType.Left : NodeType.Right;
					return new NodeRawInfo(edge.Locator.Time, type, clonedNode, edge.Locator.Track);
				case DirectNodeComponent direct:
					type = direct.Locator.IsPos ? NodeType.Pos : NodeType.Width;
					return new NodeRawInfo(direct.Locator.Time, type, clonedNode, direct.Locator.Track);
				default:
					throw new NotSupportedException();
			}
		}

		public event EventHandler? OnComponentUpdated;
		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);
	}
}