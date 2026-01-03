#nullable enable

using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;

namespace MusicGame.Models.Track
{
	public interface ITrack : IChartModel
	{
		public T3Time TimeStart { get; set; }

		public T3Time TimeEnd { get; set; }

		public ITrackMovement Movement { get; set; }

		public void Shift(float distance);
	}
}