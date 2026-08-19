#nullable enable

using System;
using MusicGame.Models.Track.Movement;
using T3Framework.Static.Easing;
using T3Framework.Static.Movement;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawMoveItem
	{
		public string type { get; }

		public int time { get; }

		public float position { get; }

		public int ease { get; }

		public float startTimeFactor { get; }

		public float startPositionFactor { get; }

		public float endTimeFactor { get; }

		public float endPositionFactor { get; }

		private RawMoveItem(string type, int time, float position, int ease,
			float startTimeFactor, float startPositionFactor, float endTimeFactor, float endPositionFactor)
		{
			this.type = type;
			this.time = time;
			this.position = position;
			this.ease = ease;
			this.startTimeFactor = startTimeFactor;
			this.startPositionFactor = startPositionFactor;
			this.endTimeFactor = endTimeFactor;
			this.endPositionFactor = endPositionFactor;
		}

		public static RawMoveItem From(IPositionMoveItem<float> item, int time)
		{
			return item switch
			{
				V1EMoveItem ease => new RawMoveItem("ease", time, ease.Position, (int)ease.Ease, 0, 0, 0, 0),
				V1BMoveItem bezier => new RawMoveItem("bezier", time, bezier.Position, 0,
					bezier.StartControlFactor.x, bezier.StartControlFactor.y,
					bezier.EndControlFactor.x, bezier.EndControlFactor.y),
				_ => throw new InvalidOperationException($"Unsupported move item: {item.GetType()}")
			};
		}

		public static int opposite(int ease) => (int)((Eases)ease).Opposite();

		public static float calcCoord(int ease, float left, float right, float t) => ((Eases)ease).CalcCoord(left, right, t);
	}
}
