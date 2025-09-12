using System;
using MusicGame.Components.Movement;
using T3Framework.Runtime;

namespace MusicGame.Components.Tracks.Movement
{
	public interface ITrackMovement : ISerializable
	{
		public event Action OnTrackMovementUpdated;

		IMovement<float> Movement1 { get; set; }

		IMovement<float> Movement2 { get; set; }

		float GetPos(T3Time time);

		float GetWidth(T3Time time);

		float GetLeftPos(T3Time time) => GetPos(time) - GetWidth(time) / 2;

		float GetRightPos(T3Time time) => GetPos(time) + GetWidth(time) / 2;

		ITrackMovement Clone(T3Time timeOffset, float positionOffset);
	}
}