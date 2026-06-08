#nullable enable

using System;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteRawInfo : NoteRawInfo
	{
		public NotifiableProperty<float> Position { get; }

		public NotifiableProperty<float> Width { get; }

		public DraftNoteRawInfo(T3Time timeJudge, T3Time timeEnd, T3Flag noteFlag, ChartComponent? parent,
			float position, float width)
			: base(timeJudge, timeEnd, noteFlag, parent)
		{
			Position = new(position);
			Width = new(width);
			Position.PropertyChanged += (_, _) => UpdateNotify();
			Width.PropertyChanged += (_, _) => UpdateNotify();
		}

		public override INote GenerateModel()
		{
			return NoteFlag.Value switch
			{
				T3Flag.Tap => new DraftHit(TimeJudge.Value, HitType.Tap, Position.Value, Width.Value),
				T3Flag.Slide => new DraftHit(TimeJudge.Value, HitType.Slide, Position.Value, Width.Value),
				T3Flag.Hold => new DraftHold(TimeJudge.Value, TimeEnd.Value, Position.Value, Width.Value),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public override ChartComponent UpdateModel(ChartComponent note)
		{
			if (note.Model is ISolitaryNote solitary)
			{
				solitary.Position = Position.Value;
				solitary.Width = Width.Value;
			}

			return base.UpdateModel(note);
		}

		public new static DraftNoteRawInfo? FromComponent(ChartComponent note)
		{
			if (note.Model is not ISolitaryNote model) return null;
			return new DraftNoteRawInfo(model.TimeJudge, model.TimeMax, model switch
			{
				DraftHit { Type: HitType.Tap } => T3Flag.Tap,
				DraftHit { Type: HitType.Slide } => T3Flag.Slide,
				DraftHold => T3Flag.Hold,
				_ => throw new ArgumentOutOfRangeException()
			}, note.Parent, model.Position, model.Width);
		}
	}
}