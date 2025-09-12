using System;

namespace T3Framework.Runtime.Timer
{
	public interface ITimer : IDisposable
	{
		TimerState State { get; }

		void Pause();

		void Resume();

		void Start();

		void Stop();

		void Reset();
	}
}