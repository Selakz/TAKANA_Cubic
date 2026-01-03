using T3Framework.Runtime;

namespace T3Framework.Static.Movement
{
	public interface IMovement<TPosition>
	{
		public TPosition GetPos(T3Time time);

		public void Nudge(T3Time distance);

		public void Shift(TPosition offset);
	}
}