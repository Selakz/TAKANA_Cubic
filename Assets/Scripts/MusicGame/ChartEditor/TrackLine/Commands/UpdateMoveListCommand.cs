using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Level;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.Commands
{
	/// <summary>
	/// null OldItem means insertion; null NewItem means removal.
	/// </summary>
	public class UpdateMoveListArg : IUpdateMovementArg
	{
		public bool IsFirst { get; }

		public IMoveItem OldItem { get; }

		public IMoveItem NewItem { get; }

		// TODO: Finish actions
		public Action<IMovement<float>> DoAction { get; private set; }

		public Action<IMovement<float>> UndoAction { get; private set; }

		public UpdateMoveListArg(bool isFirst, IMoveItem oldItem, IMoveItem newItem)
		{
			IsFirst = isFirst;
			OldItem = oldItem;
			NewItem = newItem;
		}
	}


	public class UpdateMoveListCommand : ISetInitCommand<Track>
	{
		public string Name => "Update MoveList Time";

		private Track track;
		private readonly List<UpdateMoveListArg> args;
		private bool hasExecuted;
		private T3Time oldTrackStartTime;
		private T3Time newTrackStartTime = T3Time.MaxValue;
		private T3Time oldTrackEndTime;
		private T3Time newTrackEndTime = T3Time.MinValue;
		private UpdateComponentsCommand updateTrackCommand;

		public UpdateMoveListCommand(IEnumerable<IUpdateMovementArg> args)
		{
			this.args = new();
			foreach (var arg in args)
			{
				// TODO: Allow other IUpdateMovementArg
				if (arg is not UpdateMoveListArg a) continue;
				this.args.Add(a);
			}
		}

		public bool SetInit(Track track)
		{
			if (hasExecuted)
			{
				Debug.LogError("Cannot set track after the command has been executed.");
				return false;
			}

			this.track = track;
			if (track.Movement.Movement1 is not IMoveList moveList1 ||
			    track.Movement.Movement2 is not IMoveList moveList2) return false;
			var enumerate = args.ToList();
			foreach (var arg in enumerate)
			{
				var validateMoveList = arg.IsFirst ? moveList1 : moveList2;

				// 0. Check if any old item does not exist.
				if (arg.OldItem is not null &&
				    (!validateMoveList.TryGet(arg.OldItem.Time, out var v) || !v.Equals(arg.OldItem)))
				{
					return false;
				}

				// 1. Check if any new item overwrites existing items.
				bool isChangeTime = arg.NewItem is not null &&
				                    (arg.OldItem is null || arg.NewItem.Time != arg.OldItem.Time);
				if (isChangeTime && validateMoveList.TryGet(arg.NewItem.Time, out var existingItem))
				{
					if (!args.Any(a =>
						    a.IsFirst == arg.IsFirst && existingItem.Equals(a.OldItem) &&
						    (a.NewItem is null || a.NewItem.Time != existingItem.Time)))
					{
						return false;
					}
				}

				// 2. Check if track's start/end time should change.
				if (arg.OldItem is not null && arg.NewItem is not null && arg.OldItem.Time == track.TimeInstantiate)
				{
					newTrackStartTime = Mathf.Min(newTrackStartTime, arg.NewItem.Time);
					var oppositeMoveList = arg.IsFirst ? moveList2 : moveList1;
					if (oppositeMoveList.TryGet(track.TimeInstantiate, out var startItem) && !args.Any(a =>
						    a.IsFirst == !arg.IsFirst && startItem.Equals(a.OldItem)))
					{
						args.Add(new(!arg.IsFirst, startItem, startItem.SetTime(newTrackStartTime)));
					}
				}

				if (arg.OldItem is not null && arg.NewItem is not null && arg.OldItem.Time == track.TimeEnd)
				{
					newTrackEndTime = Mathf.Max(newTrackEndTime, arg.NewItem.Time);
					var oppositeMoveList = arg.IsFirst ? moveList2 : moveList1;
					if (oppositeMoveList.TryGet(track.TimeEnd, out var endItem) && !args.Any(a =>
						    a.IsFirst == !arg.IsFirst && endItem.Equals(a.OldItem)))
					{
						args.Add(new(!arg.IsFirst, endItem, endItem.SetTime(newTrackEndTime)));
					}
				}
			}

			// 3. If track's time does change, see if it can change legally.
			if (newTrackStartTime != T3Time.MaxValue)
			{
				// TODO: Replace with new TimeStart
				if (IEditingChartManager.Instance.GetChildrenComponents(track.Id)
				    .Any(c => c is EditingNote e && e.Note.TimeJudge < newTrackStartTime))
				{
					return false;
				}

				oldTrackStartTime = track.TimeInstantiate;
			}


			if (newTrackEndTime != T3Time.MinValue)
			{
				if (IEditingChartManager.Instance.GetChildrenComponents(track.Id)
				    .Any(c => c is EditingNote e && e.TimeEnd > newTrackEndTime))
				{
					return false;
				}

				oldTrackEndTime = track.TimeEnd;
			}

			return true;
		}

		public void Do()
		{
			if (track == null)
			{
				Debug.LogWarning("No track is specified for this command");
				return;
			}

			if (track.Movement.Movement1 is not IMoveList moveList1 ||
			    track.Movement.Movement2 is not IMoveList moveList2) return;

			hasExecuted = true;
			foreach (var arg in args)
			{
				if (arg.OldItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Remove(arg.OldItem.Time);
				}
			}

			foreach (var arg in args)
			{
				if (arg.NewItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Insert(arg.NewItem);
				}
			}

			if (newTrackStartTime != T3Time.MaxValue) track.TimeInstantiate = newTrackStartTime;
			if (newTrackEndTime != T3Time.MinValue) track.TimeEnd = newTrackEndTime;
			IEditingChartManager.Instance.UpdateComponent(track.Id);
		}

		public void Undo()
		{
			if (track == null)
			{
				Debug.LogWarning("No track is specified for this command");
				return;
			}

			if (track.Movement.Movement1 is not IMoveList moveList1 ||
			    track.Movement.Movement2 is not IMoveList moveList2) return;

			foreach (var arg in args)
			{
				if (arg.NewItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Remove(arg.NewItem.Time);
				}
			}

			foreach (var arg in args)
			{
				if (arg.OldItem is not null)
				{
					var moveList = arg.IsFirst ? moveList1 : moveList2;
					moveList.Insert(arg.OldItem);
				}
			}

			if (newTrackStartTime != T3Time.MaxValue) track.TimeInstantiate = oldTrackStartTime;
			if (newTrackEndTime != T3Time.MinValue) track.TimeEnd = oldTrackEndTime;
			IEditingChartManager.Instance.UpdateComponent(track.Id);
		}
	}
}