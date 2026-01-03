#nullable enable

using System;
using T3Framework.Runtime;

namespace T3Framework.Static.Movement
{
	public class FallbackMovement<TPosition> : IMovement<TPosition>
	{
#pragma warning disable CS0067
		public event Action? OnMovementUpdated;
#pragma warning restore CS0067

		public static FallbackMovement<TPosition> Instance { get; } = new();

		public TPosition GetPos(T3Time time) => default!;

		public void Nudge(T3Time distance)
		{
		}

		public void Shift(TPosition offset)
		{
		}

		public IMovement<TPosition> Clone(T3Time timeOffset, TPosition positionOffset) => Instance;
	}
}