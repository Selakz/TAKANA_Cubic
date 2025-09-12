#nullable enable

using System;
using UnityEngine;
using UnityEngine.Events;

namespace T3Framework.Runtime.Timer
{
	public class TriggerTimer : ITimer
	{
		public event UnityAction? OnUpdate;

		public event UnityAction? OnTrigger;

		public TimerState State { get; private set; }

		public string? Name { get; set; }

		public T3Time TimeDelta { get; set; }

		public T3Time StartTime { get; private set; }

		public bool ShouldIgnoreTimeScale { get; set; }

		public bool ShouldRepeat { get; set; }

		public TriggerTimer(T3Time timeDelta)
		{
			TimeDelta = timeDelta;
			State = TimerState.Ready;
		}

		public void Update()
		{
			if (State == TimerState.Active)
			{
				if ((ShouldIgnoreTimeScale ? Time.unscaledTime : Time.time) >= StartTime + TimeDelta) Trigger();
				OnUpdate?.Invoke();
			}
		}

		public void Start()
		{
			Updater.OnUpdate -= Update;
			StartTime = ShouldIgnoreTimeScale ? Time.unscaledTime : Time.time;
			State = TimerState.Active;
			Updater.OnUpdate += Update;
		}

		public void Stop()
		{
			State = TimerState.Stop;
			Updater.OnUpdate -= Update;
		}

		public void Reset()
		{
			StartTime = ShouldIgnoreTimeScale ? Time.unscaledTime : Time.time;
		}

		private void Trigger()
		{
			if (ShouldRepeat) Reset();
			else Stop();
			OnTrigger?.Invoke();
		}

		public void Pause()
		{
			// State = TimerState.Pause;
			throw new NotImplementedException();
		}

		public void Resume()
		{
			// State = TimerState.Active;
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			Stop();
			OnUpdate = null;
			OnTrigger = null;
		}
	}
}