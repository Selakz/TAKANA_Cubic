#nullable enable

using UnityEngine.InputSystem;

namespace DLCore.Gameplay.Judge
{
	public enum DLKeyPhase
	{
		Began,
		Stationary,
		Ended
	}

	public readonly struct DLKeyInput
	{
		public Key Key { get; }

		public DLKeyPhase Phase { get; }

		/// <summary> The realtime when this input is detected, for converting to chart time. </summary>
		public double InputTime { get; }

		public DLKeyInput(Key key, DLKeyPhase phase, double inputTime)
		{
			Key = key;
			Phase = phase;
			InputTime = inputTime;
		}
	}
}
