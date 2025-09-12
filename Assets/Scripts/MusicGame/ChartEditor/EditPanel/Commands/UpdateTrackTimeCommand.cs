using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Level;
using MusicGame.Components.Movement;
using MusicGame.Components.Tracks;
using T3Framework.Runtime;

namespace MusicGame.ChartEditor.EditPanel.Commands
{
	public class UpdateTrackTimeStartCommand : ICommand
	{
		public string Name => $"Update Track {track.Id}'s Time Start";

		private readonly Track track;
		private readonly T3Time oldTimeStart;
		private readonly T3Time newTimeStart;

		public UpdateTrackTimeStartCommand(Track track, T3Time newTimeStart)
		{
			this.track = track;
			oldTimeStart = track.TimeInstantiate;
			this.newTimeStart = newTimeStart;
		}

		public void Do()
		{
			track.TimeInstantiate = newTimeStart;
			if (track.Movement.Movement1 is IMoveList leftMoveList &&
			    leftMoveList.TryGet(oldTimeStart, out var leftItem))
			{
				leftMoveList.Remove(leftItem.Time);
				leftMoveList.Insert(leftItem.SetTime(newTimeStart));
			}

			if (track.Movement.Movement1 is IMoveList rightMoveList &&
			    rightMoveList.TryGet(oldTimeStart, out var rightItem))
			{
				rightMoveList.Remove(rightItem.Time);
				rightMoveList.Insert(rightItem.SetTime(newTimeStart));
			}

			IEditingChartManager.Instance.UpdateComponent(track.Id);
		}

		public void Undo()
		{
			track.TimeInstantiate = oldTimeStart;
			if (track.Movement.Movement1 is IMoveList leftMoveList &&
			    leftMoveList.TryGet(newTimeStart, out var leftItem))
			{
				leftMoveList.Remove(leftItem.Time);
				leftMoveList.Insert(leftItem.SetTime(oldTimeStart));
			}

			if (track.Movement.Movement2 is IMoveList rightMoveList &&
			    rightMoveList.TryGet(newTimeStart, out var rightItem))
			{
				rightMoveList.Remove(rightItem.Time);
				rightMoveList.Insert(rightItem.SetTime(oldTimeStart));
			}

			IEditingChartManager.Instance.UpdateComponent(track.Id);
		}
	}

	public class UpdateTrackTimeEndCommand : ICommand
	{
		public string Name => $"Update Track {track.Id}'s Time End";

		private readonly Track track;
		private readonly T3Time oldTimeEnd;
		private readonly T3Time newTimeEnd;

		public UpdateTrackTimeEndCommand(Track track, T3Time newTimeEnd)
		{
			this.track = track;
			oldTimeEnd = track.TimeEnd;
			this.newTimeEnd = newTimeEnd;
		}

		public void Do()
		{
			track.TimeEnd = newTimeEnd;
			if (track.Movement.Movement1 is IMoveList leftMoveList &&
			    leftMoveList.TryGet(oldTimeEnd, out var leftItem))
			{
				leftMoveList.Remove(leftItem.Time);
				leftMoveList.Insert(leftItem.SetTime(newTimeEnd));
			}

			if (track.Movement.Movement2 is IMoveList rightMoveList &&
			    rightMoveList.TryGet(oldTimeEnd, out var rightItem))
			{
				rightMoveList.Remove(rightItem.Time);
				rightMoveList.Insert(rightItem.SetTime(newTimeEnd));
			}

			IEditingChartManager.Instance.UpdateComponent(track.Id);
		}

		public void Undo()
		{
			track.TimeEnd = oldTimeEnd;
			if (track.Movement.Movement1 is IMoveList leftMoveList &&
			    leftMoveList.TryGet(newTimeEnd, out var leftItem))
			{
				leftMoveList.Remove(leftItem.Time);
				leftMoveList.Insert(leftItem.SetTime(oldTimeEnd));
			}

			if (track.Movement.Movement1 is IMoveList rightMoveList &&
			    rightMoveList.TryGet(newTimeEnd, out var rightItem))
			{
				rightMoveList.Remove(rightItem.Time);
				rightMoveList.Insert(rightItem.SetTime(oldTimeEnd));
			}

			IEditingChartManager.Instance.UpdateComponent(track.Id);
		}
	}
}