#nullable enable

using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine.Events;

namespace T3Framework.Preset.Event
{
	public class DropdownRegistrar : IEventRegistrar
	{
		private readonly TMP_Dropdown dropdown;
		private readonly UnityAction<int> action;

		public DropdownRegistrar(TMP_Dropdown dropdown, UnityAction<int> action)
		{
			this.dropdown = dropdown;
			this.action = action;
		}

		public void Register()
		{
			dropdown.onValueChanged.AddListener(action);
		}

		public void Unregister()
		{
			dropdown.onValueChanged.RemoveListener(action);
		}
	}
}