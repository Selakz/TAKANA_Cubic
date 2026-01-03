#nullable enable

using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine.Events;

// ReSharper disable InconsistentNaming
namespace T3Framework.Preset.Event
{
	public readonly struct InputFieldRegistrar : IEventRegistrar
	{
		public enum RegisterTarget
		{
			OnEndEdit,
			OnSelect,
			OnDeselect,
			OnValueChanged,
		}

		private readonly TMP_InputField inputField;
		private readonly RegisterTarget registerTarget;
		private readonly UnityAction<string> action;

		public InputFieldRegistrar(TMP_InputField inputField, RegisterTarget registerTarget, UnityAction<string> action)
		{
			this.inputField = inputField;
			this.registerTarget = registerTarget;
			this.action = action;
		}

		public void Register()
		{
			UnityEvent<string> targetEvent = registerTarget switch
			{
				RegisterTarget.OnEndEdit => inputField.onEndEdit,
				RegisterTarget.OnSelect => inputField.onSelect,
				RegisterTarget.OnDeselect => inputField.onDeselect,
				RegisterTarget.OnValueChanged => inputField.onValueChanged,
				_ => null!
			};

			targetEvent?.AddListener(action);
		}

		public void Unregister()
		{
			UnityEvent<string> targetEvent = registerTarget switch
			{
				RegisterTarget.OnEndEdit => inputField.onEndEdit,
				RegisterTarget.OnSelect => inputField.onSelect,
				RegisterTarget.OnDeselect => inputField.onDeselect,
				RegisterTarget.OnValueChanged => inputField.onValueChanged,
				_ => null!
			};

			targetEvent?.RemoveListener(action);
		}
	}
}