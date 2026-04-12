#nullable enable

using System;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public enum TrackType
	{
		Edge,
		Direct
	}

	// Actually it should be named to ChartEditService
	public class ChartEditSystem : HierarchySystem<ChartEditSystem>
	{
		// Serializable and Public
		public NotifiableProperty<TrackType> SelectedTrackType { get; } = new(TrackType.Edge);

		// Private
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly ChartSelectDataset dataset = default!;

		public ICommand? CreateHit(HitType type, T3Time time)
		{
			if (levelInfo.Value?.Chart is not { } chart) return null;
			if (dataset.CurrentSelecting.Value is not { Model: ITrack model } track) return null;
			if (track.GetLayerInfo()?.IsDecoration ?? false)
			{
				return new LogCommand("Notice", "Edit_CreateFailForDecoration", T3LogType.Info);
			}

			if (time < model.TimeStart || time > model.TimeEnd)
			{
				return new LogCommand("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
			}

			Hit hit = new Hit(time, type);
			return new AddComponentCommand(chart, hit, dataset.CurrentSelecting.Value);
		}

		public ICommand? CreateHold(T3Time timeJudge, T3Time timeEnd)
		{
			if (levelInfo.Value?.Chart is not { } chart) return null;
			if (dataset.CurrentSelecting.Value is not { Model: ITrack model } track) return null;
			if (track.GetLayerInfo()?.IsDecoration ?? false)
			{
				return new LogCommand("Notice", "Edit_CreateFailForDecoration", T3LogType.Info);
			}

			timeEnd = Mathf.Min(timeEnd, model.TimeEnd);
			if (timeJudge > timeEnd)
			{
				return new LogCommand("Notice", "Edit_HoldTimeError", T3LogType.Warn);
			}

			if (timeJudge < model.TimeStart || timeEnd > model.TimeEnd)
			{
				return new LogCommand("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
			}

			Hold hold = new Hold(timeJudge, timeEnd);
			return new AddComponentCommand(chart, hold, dataset.CurrentSelecting.Value);
		}

		public ICommand? NudgeHoldJudge(ChartComponent hold, T3Time distance)
		{
			if (hold.Model is not Hold model) return null;

			var newTimeJudge = model.TimeJudge + distance;

			if (newTimeJudge > model.TimeEnd)
			{
				return new LogCommand("Notice", "Edit_HoldTimeError", T3LogType.Warn);
			}

			if (hold.Parent is not null && hold.Parent.Model.TimeMin > newTimeJudge)
			{
				return new LogCommand("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
			}

			return new UpdateComponentCommand(hold,
				_ => model.NudgeJudge(distance),
				_ => model.NudgeJudge(-distance));
		}

		public ICommand? NudgeHoldEnd(ChartComponent hold, T3Time distance)
		{
			if (hold.Model is not Hold model) return null;

			var newTimeEnd = model.TimeEnd + distance;

			if (model.TimeJudge > newTimeEnd)
			{
				return new LogCommand("Notice", "Edit_HoldTimeError", T3LogType.Warn);
			}

			if (hold.Parent is not null && newTimeEnd > hold.Parent.Model.TimeMax)
			{
				return new LogCommand("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
			}

			return new UpdateComponentCommand(hold,
				_ => model.NudgeEnd(distance),
				_ => model.NudgeEnd(-distance));
		}

		public ICommand? NudgeNote(ChartComponent component, T3Time distance)
		{
			if (component.Model is not INote) return null;

			if (!component.IsWithinParentRange(distance))
			{
				return new LogCommand("Notice", "Edit_NoteTimeOutOfRange", T3LogType.Warn);
			}

			return new UpdateComponentCommand(c => c.Nudge(distance), c => c.Nudge(-distance), component);
		}

		public ICommand? NudgeNoteJudge(ChartComponent component, T3Time distance)
		{
			if (component.Model is not INote) return null;

			return component.Model switch
			{
				Hit => NudgeNote(component, distance),
				Hold => NudgeHoldJudge(component, distance),
				_ => null
			};
		}

		public ICommand? CreateTrack(T3Time timeStart, T3Time timeEnd, float left, float right)
		{
			if (levelInfo.Value?.Chart is not { } chart) return null;
			if (timeStart > timeEnd)
			{
				return new LogCommand("Notice", "Edit_TrackTimeError", T3LogType.Warn);
			}

			ChartPosMoveList moveList1 = new();
			ChartPosMoveList moveList2 = new();

			switch (SelectedTrackType.Value)
			{
				case TrackType.Edge:
					moveList1.Insert(timeStart, new V1EMoveItem(left, Eases.Unmove));
					moveList1.Insert(timeEnd, new V1EMoveItem(left, Eases.Unmove));
					moveList2.Insert(timeStart, new V1EMoveItem(right, Eases.Unmove));
					moveList2.Insert(timeEnd, new V1EMoveItem(right, Eases.Unmove));
					break;
				case TrackType.Direct:
					moveList1.Insert(timeStart, new V1EMoveItem((left + right) / 2, Eases.Unmove));
					moveList1.Insert(timeEnd, new V1EMoveItem((left + right) / 2, Eases.Unmove));
					moveList2.Insert(timeStart, new V1EMoveItem(Mathf.Abs(right - left), Eases.Unmove));
					moveList2.Insert(timeEnd, new V1EMoveItem(Mathf.Abs(right - left), Eases.Unmove));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			Track track = new Track(timeStart, timeEnd)
			{
				Movement = SelectedTrackType.Value switch
				{
					TrackType.Edge => new TrackEdgeMovement(moveList1, moveList2),
					TrackType.Direct => new TrackDirectMovement(moveList1, moveList2),
					_ => throw new ArgumentOutOfRangeException()
				}
			};
			return new AddComponentCommand(chart, track, chart.DefaultJudgeLine());
		}

		public ICommand? DeleteSelected()
		{
			return dataset.Count > 0
				? new BatchCommand(dataset.Select(component => new DeleteComponentCommand(component)),
					"Delete Selected")
				: null;
		}
	}
}