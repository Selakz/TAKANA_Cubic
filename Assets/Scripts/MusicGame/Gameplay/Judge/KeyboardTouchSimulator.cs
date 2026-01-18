using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MusicGame.Gameplay.Judge
{
	public class KeyboardTouchSimulator : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Key triggerKey = Key.Space;
		[SerializeField] private Vector2[] positions;

		// Private
		private Touchscreen virtualTouchscreen;

		// Defined Functions
		private void SendTouch(TouchPhase phase)
		{
			for (int i = 0; i < positions.Length; i++)
			{
				var state = new TouchState
				{
					touchId = i + 1,
					phase = phase,
					position = positions[i],
					startTime = Time.realtimeSinceStartupAsDouble,
					isPrimaryTouch = i == 0,
					pressure = phase == TouchPhase.Ended ? 0f : 1f
				};

				InputSystem.QueueStateEvent(virtualTouchscreen, state);
			}
		}
		
		// System Functions
		void OnEnable()
		{
			virtualTouchscreen = InputSystem.AddDevice<Touchscreen>("VirtualTouchscreen");
		}

		void OnDisable()
		{
			if (virtualTouchscreen != null) InputSystem.RemoveDevice(virtualTouchscreen);
		}

		void Update()
		{
			var keyboard = Keyboard.current;
			if (keyboard == null) return;

			if (keyboard[triggerKey].wasPressedThisFrame)
			{
				SendTouch(TouchPhase.Began);
			}
			else if (keyboard[triggerKey].wasReleasedThisFrame)
			{
				SendTouch(TouchPhase.Ended);
			}
			else if (keyboard[triggerKey].isPressed)
			{
				SendTouch(TouchPhase.Stationary);
			}
		}
	}
}