#nullable enable

using System;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;
using T3Framework.Static.Movement;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeRawInfo : IComponent
	{
		public NotifiableProperty<T3Time> Time { get; }

		public NotifiableProperty<TrackNodeType> Type { get; }

		public NotifiableProperty<IPositionMoveItem<float>> Node { get; }

		public NotifiableProperty<ChartComponent> Parent { get; }

		public NodeRawInfo(T3Time time, TrackNodeType type, IPositionMoveItem<float> node, ChartComponent parent)
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

		public static NodeRawInfo FromComponent(TrackNodeComponent component) =>
			new(component.Time.Value, component.Type, component.Model.Clone(), component.Track);

		public event EventHandler? OnComponentUpdated;
		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);
	}
}
