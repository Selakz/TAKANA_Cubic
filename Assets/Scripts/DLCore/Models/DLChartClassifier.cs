#nullable enable

using System;
using DLCore.Models.Note;
using DLCore.Models.Track;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;

namespace DLCore.Models
{
	[Flags]
	public enum DLFlag
	{
		None = 0,
		Note = 1 << 0,
		Tap = 1 << 1,
		Slide = 1 << 2,
		Hold = 1 << 3,
		Track = 1 << 10,
		JudgeLine = 1 << 20,
		Attached = 1 << 28,
		Free = 1 << 29,
		Draft = 1 << 30,
		Live = 1 << 31 // Contrary to Draft
	}

	public class DLChartClassifier : IClassifier<DLFlag>
	{
		public static DLChartClassifier Instance { get; } = new Lazy<DLChartClassifier>(() => new()).Value;

		public DLFlag Classify(IComponent component)
		{
			if (component is not ChartComponent chartComponent) return DLFlag.None;
			var model = chartComponent.Model;
			return model switch
			{
				DraftHit { Type: HitType.Tap } => DLFlag.Draft | DLFlag.Note | DLFlag.Free | DLFlag.Tap,
				DraftHit { Type: HitType.Slide } => DLFlag.Draft | DLFlag.Note | DLFlag.Free | DLFlag.Slide,
				DraftHold => DLFlag.Draft | DLFlag.Note | DLFlag.Free | DLFlag.Hold,
				DLFreeHit { Type: HitType.Tap } => DLFlag.Live | DLFlag.Note | DLFlag.Free | DLFlag.Tap,
				DLFreeHit { Type: HitType.Slide } => DLFlag.Live | DLFlag.Note | DLFlag.Free | DLFlag.Slide,
				DLFreeHold => DLFlag.Live | DLFlag.Note | DLFlag.Free | DLFlag.Hold,
				DLHit { Type: HitType.Tap } => DLFlag.Live | DLFlag.Note | DLFlag.Attached | DLFlag.Tap,
				DLHit { Type: HitType.Slide } => DLFlag.Live | DLFlag.Note | DLFlag.Attached | DLFlag.Slide,
				DLHold => DLFlag.Live | DLFlag.Note | DLFlag.Attached | DLFlag.Hold,
				DLTrack => DLFlag.Live | DLFlag.Track,
				StaticJudgeLine => DLFlag.Live | DLFlag.JudgeLine,
				_ => DLFlag.None
			};
		}

		public bool IsOfType(IComponent component, DLFlag type) => Classify(component).HasFlag(type);

		/// <summary>
		/// subType: more general, i.e. T3Flag.Note<br/>
		/// type: more specific, i.e. T3Flag.Live | T3Flag.Note | T3Flag.Tap<br/>
		/// The "sub" here means that subType is a subset of type. It may be a little weird...
		/// </summary>
		public bool IsSubType(DLFlag subType, DLFlag type) => type.HasFlag(subType);
	}
}