namespace T3Framework.Runtime.Event
{
	/// <summary> Not thread safe </summary>
	public class CounterArg
	{
		public int Counts { get; private set; } = 0;

		public void AddCount() => Counts++;
	}
}