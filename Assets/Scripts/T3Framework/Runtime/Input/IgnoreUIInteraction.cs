#nullable enable

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace T3Framework.Runtime.Input
{
	/// <summary>
	/// Start when key pressed and modifiers matched, perform when key released and modifiers matched, cancel when key released and modifiers not matched
	/// </summary>
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class IgnoreUIInteraction : IInputInteraction
	{
		private static readonly List<RaycastResult> uiRaycastResults = new();

		public static bool IsPointerOverUI()
		{
			var eventData = new PointerEventData(EventSystem.current)
			{
				position = Pointer.current.position.ReadValue()
			};

			EventSystem.current.RaycastAll(eventData, uiRaycastResults);
			return uiRaycastResults.Count > 0;
		}

		public void Process(ref InputInteractionContext context)
		{
			switch (context.phase)
			{
				case InputActionPhase.Waiting:
					if (context.ControlIsActuated() && !IsPointerOverUI()) context.Started();
					break;
				case InputActionPhase.Started:
					if (!context.ControlIsActuated()) context.Performed();
					break;
				case InputActionPhase.Performed:
					break;
				case InputActionPhase.Canceled:
					break;
				case InputActionPhase.Disabled:
					break;
			}
		}

		public void Reset()
		{
		}

		static IgnoreUIInteraction()
		{
			InputSystem.RegisterInteraction<IgnoreUIInteraction>();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			// Will execute the static constructor as a side effect.
		}
	}
}