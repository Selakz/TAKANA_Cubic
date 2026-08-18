#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
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
			track.UpdateModel(m =>
			{
				var trackModel = (ITrack)m;
				if (isStart) trackModel.TimeStart = newTime;
				else trackModel.TimeEnd = newTime;
				MoveItem(trackModel, oldTime, newTime);
			});
			hasDone = true;
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
			});
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
