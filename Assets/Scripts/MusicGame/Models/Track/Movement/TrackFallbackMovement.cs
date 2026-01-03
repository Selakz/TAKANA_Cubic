#nullable enable

using System;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace MusicGame.Models.Track.Movement
{
	[ChartTypeMark("trackFallbackMovement")]
	public class TrackFallbackMovement : ITrackMovement
	{
#pragma warning disable CS0067
		public event Action? OnTrackMovementUpdated;
#pragma warning restore CS0067

		public static TrackFallbackMovement Instance { get; } = new();

		public IMovement<float> Movement1
		{
			get => FallbackMovement<float>.Instance;
			set { }
		}

		public IMovement<float> Movement2
		{
			get => FallbackMovement<float>.Instance;
			set { }
		}

		public float GetPos(T3Time time) => 0;

		public float GetWidth(T3Time time) => 1;

		public void Nudge(T3Time distance)
		{
		}

		public void Shift(float offset)
		{
		}

		public ITrackMovement Clone(T3Time timeOffset, float positionOffset) => Instance;

		public JObject GetSerializationToken() => new();

		public static TrackFallbackMovement Deserialize(JObject _) => Instance;
	}
}