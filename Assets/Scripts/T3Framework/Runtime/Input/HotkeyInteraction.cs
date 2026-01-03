// Modified from Arcade-Plus: https://github.com/yojohanshinwataikei/Arcade-plus

using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace T3Framework.Runtime.Input
{
	/// <summary>
	/// Start when key pressed and modifiers matched, perform when key released and modifiers matched, cancel when key released and modifiers not matched
	/// </summary>
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public class HotKeyInteraction : IInputInteraction<KeyWithModifiersData>
	{
		public bool needModifier1;
		public bool needModifier2;
		public bool needModifier3;

		public void Process(ref InputInteractionContext context)
		{
			KeyWithModifiersData data = context.ReadValue<KeyWithModifiersData>();
			bool keyPressing = data.keyValue >= InputSystem.settings.defaultButtonPressPoint;
			bool modifierMatched = needModifier1 == data.modifier1 &&
			                       needModifier2 == data.modifier2 &&
			                       needModifier3 == data.modifier3;

			switch (context.phase)
			{
				case InputActionPhase.Waiting:
					if (keyPressing && modifierMatched) context.Started();
					break;
				case InputActionPhase.Started:
					if (!keyPressing) context.Performed();
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

		static HotKeyInteraction()
		{
			InputSystem.RegisterInteraction<HotKeyInteraction>();
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void Initialize()
		{
			// Will execute the static constructor as a side effect.
		}
	}
}