#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Commands
{
	/// <summary>
	/// null OldItem means insertion; null NewItem means removal.
	/// </summary>
	public class UpdateMoveListArg
	{
		public bool IsFirst { get; }

		public T3Time? OldTime { get; }

		public IPositionMoveItem<float>? OldItem { get; set; }

		public KeyValuePair<T3Time, IPositionMoveItem<float>>? NewItem { get; }

		public UpdateMoveListArg(bool isFirst, T3Time? oldTime, KeyValuePair<T3Time, IPositionMoveItem<float>>? newItem)
		{
			IsFirst = isFirst;
			OldTime = oldTime;
			NewItem = newItem;
		}
	}


	public class UpdateMoveListCommand : ISetInitCommand<ChartComponent>
	{
		public string Name => "Update MoveList Time";

		private ChartComponent? track;
		private ITrack? model;
		private readonly List<UpdateMoveListArg> args;
		private bool hasExecuted = false;
		private T3Time oldTrackStartTime;
		private T3Time newTrackStartTime = T3Time.MaxValue;
		private T3Time oldTrackEndTime;
		private T3Time newTrackEndTime = T3Time.MinValue;

		public UpdateMoveListCommand(UpdateMoveListArg arg)
		{
			args = new() { arg };
		}

		public UpdateMoveListCommand(IEnumerable<UpdateMoveListArg> args)
		{
			this.args = args.ToList();
		}

		public bool SetInit(ChartComponent track)
		{
			if (hasExecuted)
			{
				Debug.LogError("Cannot set track after the command has been executed.");
				return false;
			}

			this.track = track;
			if (track.Model is not ITrack m) return false;
			model = m;

			if (model.Movement.Movement1 is not ChartPosMoveList moveList1 ||
			    model.Movement.Movement2 is not ChartPosMoveList moveList2) return false;
			var enumerate = args.ToList();
			foreach (var arg in enumerate)
			{
				var validateMoveList = arg.IsFirst ? moveList1 : moveList2;

				// 0. Check if any old item does not exist.
				if (arg.OldTime is not null)
				{
					if (!validateMoveList.TryGet(arg.OldTime.Value, out var item)) return false;
					arg.OldItem = item;
				}

				// 1. Check if any new item overwrites existing items.
				bool isChangeTime = arg.NewItem is not null && arg.NewItem.Value.Key != arg.OldTime;
				if (isChangeTime && validateMoveList.TryGet(arg.NewItem!.Value.Key, out _))
				{
					if (!args.Any(a =>
						    a.IsFirst == arg.IsFirst &&
						    a.OldTime == arg.NewItem.Value.Key &&
						    (a.NewItem is null || a.NewItem.Value.Key != a.OldTime)))
					{
						return false;
					}
				}

				// 2. Check if track's start/end time should change.
				if (arg.OldTime is not null && arg.NewItem is not null && arg.OldTime == model.TimeStart)
				{
					newTrackStartTime = Mathf.Min(newTrackStartTime, arg.NewItem.Value.Key);
					var oppositeMoveList = arg.IsFirst ? moveList2 : moveList1;
					if (oppositeMoveList.TryGet(model.TimeStart, out var startItem) &&
					    !args.Any(a => a.IsFirst == !arg.IsFirst && a.OldTime == model.TimeStart))
					{
						UpdateMoveListArg newArg = new(
							!arg.IsFirst, model.TimeStart, new(newTrackStartTime, startItem))
						{
							OldItem = startItem
						};
						args.Add(newArg);
					}
				}

				if (arg.OldTime is not null && arg.NewItem is not null && arg.OldTime == model.TimeEnd)
				{
					newTrackEndTime = Mathf.Max(newTrackEndTime, arg.NewItem.Value.Key);
					var oppositeMoveList = arg.IsFirst ? moveList2 : moveList1;
					if (oppositeMoveList.TryGet(model.TimeEnd, out var endItem) &&
					    !args.Any(a => a.IsFirst == !arg.IsFirst && a.OldTime == model.TimeEnd))
					{
						UpdateMoveListArg newArg = new(
							!arg.IsFirst, model.TimeEnd, new(newTrackEndTime, endItem))
						{
							OldItem = endItem
						};
						args.Add(newArg);
					}
				}
			}

			// 3. If track's time does change, see if it can change legally.
			if (newTrackStartTime != T3Time.MaxValue)
			{
				if (!track.IsNewTimeMinValid(newTrackStartTime)) return false;
				oldTrackStartTime = model.TimeStart;
			}


			if (newTrackEndTime != T3Time.MinValue)
			{
				if (!track.IsNewTimeMaxValid(newTrackEndTime)) return false;
				oldTrackEndTime = model.TimeEnd;
			}

			return true;
		}

		public void Do()
		{
			if (track is null || model is null)
			{
				Debug.LogWarning("No track is specified for this command");
				return;
			}

			if (model.Movement.Movement1 is not ChartPosMoveList moveList1 ||
			    model.Movement.Movement2 is not ChartPosMoveList moveList2) return;

			hasExecuted = true;

			foreach (var arg in args)
			{
				if (arg.OldTime is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Remove(arg.OldTime.Value);
				}
			}

			foreach (var arg in args)
			{
				if (arg.NewItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Insert(arg.NewItem.Value.Key, arg.NewItem.Value.Value);
				}
			}

			if (newTrackStartTime != T3Time.MaxValue) model.TimeStart = newTrackStartTime;
			if (newTrackEndTime != T3Time.MinValue) model.TimeEnd = newTrackEndTime;
			track.UpdateNotify();
		}

		public void Undo()
		{
			if (track is null || model is null)
			{
				Debug.LogWarning("No track is specified for this command");
				return;
			}

			if (model.Movement.Movement1 is not ChartPosMoveList moveList1 ||
			    model.Movement.Movement2 is not ChartPosMoveList moveList2) return;

			foreach (var arg in args)
			{
				if (arg.NewItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Remove(arg.NewItem.Value.Key);
				}
			}

			foreach (var arg in args)
			{
				if (arg.OldTime is not null && arg.OldItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Insert(arg.OldTime.Value, arg.OldItem);
				}
			}

			if (newTrackStartTime != T3Time.MaxValue) model.TimeStart = oldTrackStartTime;
			if (newTrackEndTime != T3Time.MinValue) model.TimeEnd = oldTrackEndTime;
			track.UpdateNotify();
		}
	}
}