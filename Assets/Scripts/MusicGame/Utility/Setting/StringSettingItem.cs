#nullable enable

using System.ComponentModel;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class StringSettingItem : SingleValueSettingItem<string>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField valueInputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar
				(valueInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnValueInputFieldEndEdit)
		};

		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			valueInputField.text = DisplayValue;
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			valueInputField.text = DisplayValue;
		}

		// Event Handlers
		private void OnValueInputFieldEndEdit(string value)
		{
			if (DisplayValue == value) return;

			DisplayValue = value;
			Save();
		}
	}
}