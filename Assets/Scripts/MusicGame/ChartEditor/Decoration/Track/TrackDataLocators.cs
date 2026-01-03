#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Movement;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public readonly struct MovementLocator<T> : IDataLocator<T>, IEquatable<MovementLocator<T>> where T : ITrackMovement
	{
		public ChartComponent Track { get; }

		public MovementLocator(ChartComponent track)
		{
			Track = track;
		}

		public T? GetData()
		{
			if (Track.Model is not ITrack model) return default;
			return model.Movement is T movement ? movement : default;
		}

		public bool Equals(MovementLocator<T> other) => Track.Equals(other.Track);

		public bool Equals(IDataLocator<T> other) => other is MovementLocator<T> locator && Equals(locator);

		public override bool Equals(object? obj) => obj is MovementLocator<T> other && Equals(other);

		public override int GetHashCode() => Track.GetHashCode();

		public static IEnumerable<MovementLocator<T>> Factory(ChartComponent component)
		{
			yield return new MovementLocator<T>(component);
		}
	}

	public readonly struct EdgeSideMovementLocator<T> : IDataLocator<T>, IEquatable<EdgeSideMovementLocator<T>>
		where T : IMovement<float>
	{
		public ChartComponent Track { get; }
		public bool IsLeft { get; }
		private readonly MovementLocator<TrackEdgeMovement> locator;

		public EdgeSideMovementLocator(ChartComponent component, bool isLeft)
		{
			Track = component;
			IsLeft = isLeft;
			locator = new(component);
		}

		public T? GetData() => IsLeft
			? locator.GetData()?.Movement1 is T movement1
				? movement1
				: default
			: locator.GetData()?.Movement2 is T movement2
				? movement2
				: default;

		public bool Equals(EdgeSideMovementLocator<T> other) =>
			Track.Equals(other.Track) && IsLeft == other.IsLeft;

		public bool Equals(IDataLocator<T> other) => other is EdgeSideMovementLocator<T> l && Equals(l);

		public override bool Equals(object? obj) => obj is EdgeSideMovementLocator<T> other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(Track, IsLeft);

		public static IEnumerable<EdgeSideMovementLocator<T>> Factory(EdgeComponent component)
		{
			yield return new EdgeSideMovementLocator<T>(component.Locator.Track, false);
			yield return new EdgeSideMovementLocator<T>(component.Locator.Track, true);
		}
	}

	public readonly struct EdgeSideMoveItemLocator
		: IDataLocator<IPositionMoveItem<float>>, IEquatable<EdgeSideMoveItemLocator>
	{
		public ChartComponent Track { get; }

		public bool IsLeft { get; }

		public T3Time Time { get; }

		public EdgeSideMovementLocator<ChartPosMoveList> InnerLocator { get; }

		public EdgeSideMoveItemLocator(ChartComponent track, bool isLeft, T3Time time)
		{
			Track = track;
			IsLeft = isLeft;
			Time = time;
			InnerLocator = new(Track, IsLeft);
		}

		public IPositionMoveItem<float>? GetData() =>
			InnerLocator.GetData()?.TryGet(Time, out var item) == true ? item : default;

		public bool Equals(IDataLocator<IPositionMoveItem<float>> other)
			=> other is EdgeSideMoveItemLocator l && Equals(l);

		public bool Equals(EdgeSideMoveItemLocator other) =>
			Track.Equals(other.Track) && IsLeft == other.IsLeft && Time.Equals(other.Time);

		public override bool Equals(object? obj) => obj is EdgeSideMoveItemLocator other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(Track, IsLeft, Time);

		public static IEnumerable<EdgeSideMoveItemLocator> Factory(EdgePMLComponent component)
		{
			var moveList = component.Locator.GetData();
			if (moveList is null) yield break;
			foreach (var pair in moveList)
			{
				yield return new(component.Locator.Track, component.Locator.IsLeft, pair.Key);
			}
		}
	}
}