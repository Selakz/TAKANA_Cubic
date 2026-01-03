#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;

namespace MusicGame.Models
{
	[Flags]
	public enum T3Flag
	{
		None = 0,
		Note = 1 << 0,
		Tap = 1 << 1,
		Slide = 1 << 2,
		Hold = 1 << 3,
		Track = 1 << 10,
		JudgeLine = 1 << 20,
	}

	public class T3ChartClassifier : IClassifier<T3Flag>
	{
		public T3Flag Classify(IComponent component)
		{
			if (component is not ChartComponent chartComponent) return T3Flag.None;
			var model = chartComponent.Model;
			return model switch
			{
				Hit { Type: HitType.Tap } => T3Flag.Note | T3Flag.Tap,
				Hit { Type: HitType.Slide } => T3Flag.Note | T3Flag.Slide,
				Hold => T3Flag.Note | T3Flag.Hold,
				Track.Track => T3Flag.Track,
				StaticJudgeLine => T3Flag.JudgeLine,
				_ => T3Flag.None
			};
		}

		public bool IsOfType(IComponent component, T3Flag type) => Classify(component).HasFlag(type);

		public bool IsSubType(T3Flag subType, T3Flag type) => type.HasFlag(subType);
	}
}