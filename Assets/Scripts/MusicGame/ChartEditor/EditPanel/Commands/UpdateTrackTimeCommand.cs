#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace MusicGame.ChartEditor.EditPanel.Commands
{
	public class UpdateTrackTimeStartCommand : ISetInitCommand
	{
		public string Name => $"Update Track {track.Id}'s Time Start";

		private readonly ChartComponent track;
		private ITrack? model;
		private T3Time oldTimeStart;
		private readonly T3Time newTimeStart;

		public UpdateTrackTimeStartCommand(ChartComponent track, T3Time newTimeStart)
		{
			this.track = track;
			this.newTimeStart = newTimeStart;
		}

		public bool SetInit()
		{
			if (track.Model is not ITrack trackModel) return false;
			if (!track.IsNewTimeMinValid(newTimeStart)) return false;
			model = trackModel;
			oldTimeStart = model.TimeStart;
			return true;
		}

		public void Do()
		{
			track.UpdateModel(_ =>
			{
				model!.TimeStart = newTimeStart;
				if (model.Movement.Movement1 is ChartPosMoveList leftMoveList &&
				    leftMoveList.TryGet(oldTimeStart, out var leftItem))
				{
					leftMoveList.Remove(oldTimeStart);
					leftMoveList.Insert(newTimeStart, leftItem);
				}

				if (model.Movement.Movement2 is ChartPosMoveList rightMoveList &&
				    rightMoveList.TryGet(oldTimeStart, out var rightItem))
				{
					rightMoveList.Remove(oldTimeStart);
					rightMoveList.Insert(newTimeStart, rightItem);
				}
			});
		}

		public void Undo()
		{
			track.UpdateModel(_ =>
			{
				model!.TimeStart = oldTimeStart;
				if (model.Movement.Movement1 is ChartPosMoveList leftMoveList &&
				    leftMoveList.TryGet(newTimeStart, out var leftItem))
				{
					leftMoveList.Remove(newTimeStart);
					leftMoveList.Insert(oldTimeStart, leftItem);
				}

				if (model.Movement.Movement2 is ChartPosMoveList rightMoveList &&
				    rightMoveList.TryGet(newTimeStart, out var rightItem))
				{
					rightMoveList.Remove(newTimeStart);
					rightMoveList.Insert(oldTimeStart, rightItem);
				}
			});
		}
	}

	public class UpdateTrackTimeEndCommand : ISetInitCommand
	{
		public string Name => $"Update Track {track.Id}'s Time End";

		private readonly ChartComponent track;
		private ITrack? model;
		private T3Time oldTimeEnd;
		private readonly T3Time newTimeEnd;

		public UpdateTrackTimeEndCommand(ChartComponent track, T3Time newTimeEnd)
		{
			this.track = track;
			this.newTimeEnd = newTimeEnd;
		}

		public bool SetInit()
		{
			if (track.Model is not ITrack trackModel) return false;
			if (!track.IsNewTimeMaxValid(newTimeEnd)) return false;
			model = trackModel;
			oldTimeEnd = model.TimeEnd;
			return true;
		}

		public void Do()
		{
			track.UpdateModel(_ =>
			{
				model!.TimeEnd = newTimeEnd;
				if (model.Movement.Movement1 is ChartPosMoveList leftMoveList &&
				    leftMoveList.TryGet(oldTimeEnd, out var leftItem))
				{
					leftMoveList.Remove(oldTimeEnd);
					leftMoveList.Insert(newTimeEnd, leftItem);
				}

				if (model.Movement.Movement2 is ChartPosMoveList rightMoveList &&
				    rightMoveList.TryGet(oldTimeEnd, out var rightItem))
				{
					rightMoveList.Remove(oldTimeEnd);
					rightMoveList.Insert(newTimeEnd, rightItem);
				}
			});
		}

		public void Undo()
		{
			track.UpdateModel(_ =>
			{
				model!.TimeEnd = oldTimeEnd;
				if (model.Movement.Movement1 is ChartPosMoveList leftMoveList &&
				    leftMoveList.TryGet(newTimeEnd, out var leftItem))
				{
					leftMoveList.Remove(newTimeEnd);
					leftMoveList.Insert(oldTimeEnd, leftItem);
				}

				if (model.Movement.Movement2 is ChartPosMoveList rightMoveList &&
				    rightMoveList.TryGet(newTimeEnd, out var rightItem))
				{
					rightMoveList.Remove(newTimeEnd);
					rightMoveList.Insert(oldTimeEnd, rightItem);
				}
			});
		}
	}
}