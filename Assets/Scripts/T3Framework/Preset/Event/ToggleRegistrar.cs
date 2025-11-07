#nullable enable

using T3Framework.Runtime.Event;
using UnityEngine.Events;
using UnityEngine.UI;

namespace T3Framework.Preset.Event
{
	public readonly struct ToggleRegistrar : IEventRegistrar
	{
		private readonly Toggle toggle;
		private readonly UnityAction<bool> action;

		public ToggleRegistrar(Toggle toggle, UnityAction<bool> action)
		{
			this.toggle = toggle;
			this.action = action;
		}

		public void Register()
		{
			toggle.onValueChanged.AddListener(action);
		}

		public void Unregister()
		{
			toggle.onValueChanged.RemoveListener(action);
		}
	}
}