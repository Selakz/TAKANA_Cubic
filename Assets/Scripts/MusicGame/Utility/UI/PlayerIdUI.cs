#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.UI
{
	public class PlayerIdUI : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField playerIdInputField = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar(playerIdInputField,
				InputFieldRegistrar.RegisterTarget.OnEndEdit, OnPlayerIdEndEdit),
			new PropertyRegistrar<string>(ISingletonSetting<PlayfieldSetting>.Instance.PlayerId,
				value => playerIdInputField.SetTextWithoutNotify(value))
		};

		// Event Handlers
		private void OnPlayerIdEndEdit(string value)
		{
			var setting = ISingletonSetting<PlayfieldSetting>.Instance;
			if (setting.PlayerId.Value == value) return;
			setting.PlayerId.Value = value;
			ISingletonSetting<PlayfieldSetting>.SaveInstance();
		}
	}
}
