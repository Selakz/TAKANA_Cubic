#nullable enable

using MusicGame.Models.Note.Movement;
using T3Framework.Runtime;

namespace MusicGame.Models.Note
{
	public interface INote : IChartModel
	{
		public T3Time TimeJudge { get; set; }

		public INoteMovement Movement { get; }
	}
}