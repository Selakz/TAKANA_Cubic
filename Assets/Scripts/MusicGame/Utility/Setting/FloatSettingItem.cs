#nullable enable

using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class FloatSettingItem : SingleValueSettingItem<float>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField valueInputField = default!;
		[SerializeField] private TMP_Dropdown valueDropdown = default!;
		[SerializeField] private string valueFormat = "0.000";

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar
				(valueInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnValueInputFieldEndEdit),
			new DropdownRegistrar(valueDropdown, option =>
			{
				if (options is null) return;
				DisplayValue = options[option];
				Save();
			})
		};

		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			valueInputField.text = DisplayValue.ToString(valueFormat);

			var minValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MinValueAttribute>();
			if (minValueAttribute is not null) minValue = minValueAttribute.MinValue;
			var maxValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxValueAttribute>();
			if (maxValueAttribute is not null) maxValue = maxValueAttribute.MaxValue;
			var optionsAttribute = TargetPropertyInfo!.GetCustomAttribute<DropdownOptionsAttribute>();
			if (optionsAttribute is not null)
			{
				valueInputField.gameObject.SetActive(false);
				valueDropdown.gameObject.SetActive(true);
				options = valueDropdown.SetOptions(
					optionsAttribute.GetOptions<float>().ToList(), value => value.ToString(valueFormat));
				var index = Array.IndexOf(options, DisplayValue);
				if (index >= 0) valueDropdown.SetValueWithoutNotify(index);
			}
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			valueInputField.text = DisplayValue.ToString(valueFormat);
		}

		// Private
		private float minValue = float.MinValue;
		private float maxValue = float.MaxValue;
		private float[]? options;

		// Event Handlers
		private void OnValueInputFieldEndEdit(string value)
		{
			if (float.TryParse(value, out var floatValue) && !Mathf.Approximately(DisplayValue, floatValue))
			{
				floatValue = Mathf.Clamp(floatValue, minValue, maxValue);
				DisplayValue = floatValue;
				Save();
			}
			else
			{
				valueInputField.SetTextWithoutNotify(DisplayValue.ToString(valueFormat));
			}
		}
	}
}