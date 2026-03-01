#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public interface INoteRawInfoService
	{
		/// <summary> Returns null if the note is valid, otherwise returns the reason why invalid. </summary>
		public string? IsValid(NoteRawInfo info);
	}

	public class NoteRawInfo : IComponent
	{
		public NotifiableProperty<T3Time> TimeJudge { get; }

		public NotifiableProperty<T3Time> TimeEnd { get; }

		/// <summary> The value can only be single Tap, Slide, or Hold. </summary>
		public NotifiableProperty<T3Flag> NoteFlag { get; }

		public NotifiableProperty<ChartComponent?> Parent { get; }

		public NoteRawInfo(T3Time timeJudge, T3Time timeEnd, T3Flag noteFlag, ChartComponent? parent)
		{
			TimeJudge = new(timeJudge);
			TimeEnd = new(timeEnd);
			NoteFlag = new(noteFlag);
			Parent = new(parent);
			TimeJudge.PropertyChanged += (_, _) => UpdateNotify();
			TimeEnd.PropertyChanged += (_, _) => UpdateNotify();
			NoteFlag.PropertyChanged += (_, _) => UpdateNotify();
			Parent.PropertyChanged += (_, _) => UpdateNotify();
		}

		public event EventHandler? OnComponentUpdated;

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public INote GenerateModel()
		{
			return NoteFlag.Value switch
			{
				T3Flag.Tap => new Hit(TimeJudge.Value, HitType.Tap),
				T3Flag.Slide => new Hit(TimeJudge.Value, HitType.Slide),
				T3Flag.Hold => new Hold(TimeJudge.Value, TimeEnd.Value),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		/// <summary> For preview system only </summary>
		public ChartComponent UpdateModel(ChartComponent note)
		{
			var type = T3ChartClassifier.Instance.Classify(note);
			if (!T3ChartClassifier.Instance.IsSubType(NoteFlag, type))
			{
				var newModel = GenerateModel();
				newModel.Properties = note.Model.Properties;
				newModel.EditorConfig = note.Model.EditorConfig;
				ChartComponent newNote = new(newModel) { Parent = note.Parent };
				note.BelongingChart = null;
				note.UpdateNotify();
				return newNote;
			}
			else
			{
				note.Parent = Parent.Value;
				switch (note.Model)
				{
					case Hit hit:
						hit.Nudge(TimeJudge - hit.TimeJudge);
						break;
					case Hold hold:
						hold.NudgeJudge(TimeJudge - hold.TimeJudge);
						hold.NudgeEnd(TimeEnd - hold.TimeEnd);
						break;
					default:
						break;
				}

				note.UpdateNotify();
				return note;
			}
		}

		public static NoteRawInfo? FromComponent(ChartComponent note)
		{
			if (note.Model is not INote model) return null;
			return new NoteRawInfo(model.TimeJudge, model.TimeMax, model switch
			{
				Hit { Type: HitType.Tap } => T3Flag.Tap,
				Hit { Type: HitType.Slide } => T3Flag.Slide,
				Hold => T3Flag.Hold,
				_ => throw new ArgumentOutOfRangeException()
			}, note.Parent);
		}
	}
}