#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.Models.Track.Movement
{
	/// <summary> V1B: 1 Dimension, cubic bezier </summary>
	[StringTypeMark("v1b")]
	public class V1BMoveItem : IPositionMoveItem<float>, IStringSerializable
	{
		public float Position { get; set; }

		/// <summary> x: time factor, y: position factor </summary>
		public Vector2 StartControlFactor { get; set; }

		/// <summary> x: time factor, y: position factor </summary>
		public Vector2 EndControlFactor { get; set; }

		public float Add(float x, float y) => x + y;

		public float GetPosition(T3Time thisTime, T3Time targetTime, T3Time nextTime, float nextPosition)
		{
			if (thisTime == nextTime) return nextPosition;
			const int iterationTimes = 5;
			float timeT = (targetTime.Second - thisTime.Second) / (nextTime.Second - thisTime.Second);

			float factorT = timeT;
			for (int i = 0; i < iterationTimes; i++)
			{
				float currentT = CubicBezier(0, StartControlFactor.x, EndControlFactor.x, 1, factorT);
				float slope = CubicDerivative(0, StartControlFactor.x, EndControlFactor.x, 1, factorT);
				if (Mathf.Abs(slope) < 1e-6) break;

				factorT -= (currentT - timeT) / slope;
				factorT = Mathf.Clamp01(factorT);
			}

			float positionT = CubicBezier(0, StartControlFactor.y, EndControlFactor.y, 1, factorT);
			return Mathf.Lerp(Position, nextPosition, positionT);
		}

		private static float CubicBezier(float start, float startControl, float endControl, float end, float t)
		{
			float u = 1 - t;
			return u * u * u * start +
			       3 * u * u * t * startControl +
			       3 * u * t * t * endControl +
			       t * t * t * end;
		}

		private static float CubicDerivative(float start, float startControl, float endControl, float end, float t)
		{
			float u = 1 - t;
			return 3 * u * u * (startControl - start) +
			       6 * u * t * (endControl - startControl) +
			       3 * t * t * (end - endControl);
		}

		public IPositionMoveItem<float> SetPosition(float newPosition) =>
			new V1BMoveItem(newPosition, StartControlFactor, EndControlFactor);

		public V1BMoveItem(float position, Vector2 startControlFactor, Vector2 endControlFactor)
		{
			Position = position;
			StartControlFactor = startControlFactor;
			EndControlFactor = endControlFactor;
		}

		public string Serialize() =>
			$"({Position:0.00}, {StartControlFactor.x:0.00}, {StartControlFactor.y:0.00}, {EndControlFactor.x:0.00}, {EndControlFactor.y:0.00})";

		public static V1BMoveItem Deserialize(string content)
		{
			var match = RegexHelper.MatchTuple(content, 5);
			var position = float.Parse(match.Groups[1].Value);
			var startX = float.Parse(match.Groups[2].Value);
			var startY = float.Parse(match.Groups[3].Value);
			var endX = float.Parse(match.Groups[4].Value);
			var endY = float.Parse(match.Groups[5].Value);
			return new V1BMoveItem(position, new(startX, startY), new(endX, endY));
		}
	}
}