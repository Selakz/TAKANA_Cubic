#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;
using T3Framework.Static.Movement;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public enum TrackNodeType
	{
		Left,
		Right,
		Pos,
		Width
	}

	public static class TrackNodeTypeExtensions
	{
		public static bool IsFirst(this TrackNodeType type) => type is TrackNodeType.Left or TrackNodeType.Pos;

		public static bool IsEdge(this TrackNodeType type) => type is TrackNodeType.Left or TrackNodeType.Right;

		public static bool IsPosition(this TrackNodeType type) => type is not TrackNodeType.Width;
	}

	public class TrackNodeComponent : IComponent<IPositionMoveItem<float>>
	{
		public ChartComponent Track { get; }

		public TrackNodeType Type { get; }

		public bool IsFirst => Type.IsFirst();

		public bool IsEdge => Type.IsEdge();

		public bool IsPosition => Type.IsPosition();

		public ITrack TrackModel => (ITrack)Track.Model;

		public ITrackMovement Movement => TrackModel.Movement;

		public ChartPosMoveList? MoveList => (IsFirst ? Movement.Movement1 : Movement.Movement2) as ChartPosMoveList;

		public NotifiableProperty<T3Time> Time { get; }

		public NotifiableProperty<IPositionMoveItem<float>> Item { get; }

		public IPositionMoveItem<float> Model => Item.Value;

		public event EventHandler? OnComponentUpdated;

		private bool isSyncing;

		public TrackNodeComponent(ChartComponent track, TrackNodeType type, T3Time time, IPositionMoveItem<float> item)
		{
			Track = track;
			Type = type;
			Time = new(time);
			Item = new(item);
			Time.PropertyChanged += (_, _) =>
			{
				if (!isSyncing) UpdateNotify();
			};
			Item.PropertyChanged += (_, _) =>
			{
				if (!isSyncing) UpdateNotify();
			};
		}

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public void UpdateModel(Action<IPositionMoveItem<float>> action)
		{
			action.Invoke(Model);
			UpdateNotify();
		}

		internal void Sync(T3Time newTime, IPositionMoveItem<float> newItem)
		{
			isSyncing = true;
			try
			{
				Time.Value = newTime;
				Item.Value = newItem;
			}
			finally
			{
				isSyncing = false;
			}

			UpdateNotify();
		}
	}
}