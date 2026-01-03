using T3Framework.Runtime;
using T3Framework.Static.Movement;

namespace MusicGame.Models.Track.Movement
{
	public interface ITrackMovement : IChartSerializable
	{
		IMovement<float> Movement1 { get; set; }

		IMovement<float> Movement2 { get; set; }

		float GetPos(T3Time time);

		float GetWidth(T3Time time);

		float GetLeftPos(T3Time time) => GetPos(time) - GetWidth(time) / 2;

		float GetRightPos(T3Time time) => GetPos(time) + GetWidth(time) / 2;

		public void Nudge(T3Time distance);

		public void Shift(float offset);
	}
}