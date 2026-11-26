#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Easing;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.ChartEditor.EditPanel.Commands
{
	public class UpdateTrackTimeCommand : ICommand
	{
		public string Name => $"Update Track {track.Id}'s Time {(isStart ? "Start" : "End")}";

		private readonly ChartComponent track;
		private readonly bool isStart;
		private readonly T3Time newTime;
		private T3Time oldTime;
		private bool hasDone;
		private IPositionMoveItem<float>? alignedOldItem1;
		private IPositionMoveItem<float>? alignedOldItem2;

		public UpdateTrackTimeCommand(ChartComponent track, bool isStart, T3Time newTime)
		{
			this.track = track;
			this.isStart = isStart;
			this.newTime = newTime;
		}

		public void Do()
		{
			var model = (ITrack)track.Model;
			oldTime = isStart ? model.TimeStart : model.TimeEnd;
			alignedOldItem1 = null;
			alignedOldItem2 = null;
			track.UpdateModel(m =>
			{
				var trackModel = (ITrack)m;

				// Special logic: before modifying track end time, if the end-time node exists
				// and its previous node is an Unmove node, align the end-time node's position to it.
				if (!isStart)
				{
					if (trackModel.Movement.Movement1 is ChartPosMoveList moveList1)
					{
						alignedOldItem1 = AlignEndNodeToPreviousUnmove(moveList1, oldTime);
					}

					if (trackModel.Movement.Movement2 is ChartPosMoveList moveList2)
					{
						alignedOldItem2 = AlignEndNodeToPreviousUnmove(moveList2, oldTime);
					}
				}

				if (isStart) trackModel.TimeStart = newTime;
				else trackModel.TimeEnd = newTime;
				MoveItem(trackModel, oldTime, newTime);
			});
			hasDone = true;

			// Special logic helper
			static IPositionMoveItem<float>? AlignEndNodeToPreviousUnmove(ChartPosMoveList moveList, T3Time endTime)
			{
				int index = moveList.BinarySearch(endTime);
				if (index <= 0) return null;
				if (moveList[index - 1].Value is not V1EMoveItem { Ease: Eases.Unmove } prevItem) return null;
				var endItem = moveList[index].Value;
				if (endItem.Position == prevItem.Position) return null;
				moveList.Insert(endTime, endItem.SetPosition(prevItem.Position));
				return endItem;
			}
		}

		public void Undo()
		{
			if (!hasDone)
			{
				Debug.LogError("UpdateTrackTimeCommand.Undo: command has not been done yet.");
				return;
			}

			track.UpdateModel(m =>
			{
				var trackModel = (ITrack)m;
				if (isStart) trackModel.TimeStart = oldTime;
				else trackModel.TimeEnd = oldTime;
				MoveItem(trackModel, newTime, oldTime);

				// Special logic: restore the end-time node's original item before alignment.
				if (alignedOldItem1 is not null && trackModel.Movement.Movement1 is ChartPosMoveList moveList1)
				{
					moveList1.Insert(oldTime, alignedOldItem1);
				}

				if (alignedOldItem2 is not null && trackModel.Movement.Movement2 is ChartPosMoveList moveList2)
				{
					moveList2.Insert(oldTime, alignedOldItem2);
				}
			});
			alignedOldItem1 = null;
			alignedOldItem2 = null;
			hasDone = false;
		}

		private static void MoveItem(ITrack model, T3Time from, T3Time to)
		{
			if (model.Movement.Movement1 is ChartPosMoveList leftMoveList &&
			    leftMoveList.TryGet(from, out var leftItem))
			{
				leftMoveList.Remove(from);
				leftMoveList.Insert(to, leftItem);
			}

			if (model.Movement.Movement2 is ChartPosMoveList rightMoveList &&
			    rightMoveList.TryGet(from, out var rightItem))
			{
				rightMoveList.Remove(from);
				rightMoveList.Insert(to, rightItem);
			}
		}
	}
}
