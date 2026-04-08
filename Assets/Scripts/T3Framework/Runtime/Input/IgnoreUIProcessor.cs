#nullable enable

using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace T3Framework.Runtime.Input
{
	public class IgnoreUIProcessor : InputProcessor<float>
	{
		public override float Process(float value, InputControl control)
		{
			return EventSystem.current.currentSelectedGameObject
				? 0f
				: value;
		}
	}
}