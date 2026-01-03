#nullable enable

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
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Log;
using T3Framework.Static.Easing;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class ChartEditSystem : T3System
	{
		// Private
		private readonly NotifiableProperty<LevelInfo?> levelInfo;
		private readonly ChartSelectDataset dataset;

		// Defined Functions
		public ChartEditSystem(
			NotifiableProperty<LevelInfo?> levelInfo,
			ChartSelectDataset dataset) : base(true)
		{
			this.levelInfo = levelInfo;
			this.dataset = dataset;
		}

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

			ChartPosMoveList leftMoveList = new();
			leftMoveList.Insert(timeStart, new V1EMoveItem(left, Eases.Unmove));
			leftMoveList.Insert(timeEnd, new V1EMoveItem(left, Eases.Unmove));
			ChartPosMoveList rightMoveList = new();
			rightMoveList.Insert(timeStart, new V1EMoveItem(right, Eases.Unmove));
			rightMoveList.Insert(timeEnd, new V1EMoveItem(right, Eases.Unmove));
			Track track = new Track(timeStart, timeEnd)
			{
				Movement = new TrackEdgeMovement(leftMoveList, rightMoveList)
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