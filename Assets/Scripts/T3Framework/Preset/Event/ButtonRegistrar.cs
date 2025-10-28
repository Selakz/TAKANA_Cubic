#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine.Events;
using UnityEngine.UI;

namespace T3Framework.Preset.Event
{
	public readonly struct ButtonRegistrar : IEventRegistrar
	{
		private readonly Button button;
		private readonly UnityAction action;

		public ButtonRegistrar(Button button, UnityAction action)
		{
			this.button = button;
			this.action = action;
		}

		public void Register()
		{
			button.onClick.AddListener(action);
		}

		public void Unregister()
		{
			button.onClick.RemoveListener(action);
		}
	}
}