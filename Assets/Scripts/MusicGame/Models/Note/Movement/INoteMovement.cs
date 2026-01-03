using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace MusicGame.Models.Note.Movement
{
	/// <summary>
	/// Marking the movement of a note
	/// </summary>
	public interface INoteMovement : IMovement<float>, IChartSerializable
	{
		public T3Time TimeJudge { get; set; }

		/// <summary>
		/// Calculate the first time when this movement meets the comparison of given edge.
		/// </summary>
		/// <param name="edge">Edge value</param>
		/// <param name="lesserThan">true means lesser than value, false means greater than.</param>
		/// <returns>the first time which meets the comparison. T3Time.MaxValue if it never meets the comparison.</returns>
		public T3Time FirstTimeWhen(float edge, bool lesserThan);
	}
}