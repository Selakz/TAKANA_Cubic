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
	public class IntegerSettingItem : SingleValueSettingItem<int>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField valueInputField = default!;
		[SerializeField] private TMP_Dropdown valueDropdown = default!;

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

		// Private
		private int minValue = int.MinValue;
		private int maxValue = int.MaxValue;
		private int[]? options;

		// Defined Functions
		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			valueInputField.text = DisplayValue.ToString();

			var minValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MinValueAttribute>();
			if (minValueAttribute is not null) minValue = Mathf.RoundToInt(minValueAttribute.MinValue);
			var maxValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxValueAttribute>();
			if (maxValueAttribute is not null) maxValue = Mathf.RoundToInt(maxValueAttribute.MaxValue);
			var optionsAttribute = TargetPropertyInfo!.GetCustomAttribute<DropdownOptionsAttribute>();
			if (optionsAttribute is not null)
			{
				valueInputField.gameObject.SetActive(false);
				valueDropdown.gameObject.SetActive(true);
				options = valueDropdown.SetOptions(
					optionsAttribute.GetOptions<int>().ToList(), value => value.ToString());
				var index = Array.IndexOf(options, DisplayValue);
				if (index >= 0) valueDropdown.SetValueWithoutNotify(index);
			}
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			valueInputField.text = DisplayValue.ToString();
			if (options is not null)
			{
				var index = Array.IndexOf(options, DisplayValue);
				if (index >= 0) valueDropdown.SetValueWithoutNotify(index);
			}
		}

		// Event Handlers
		private void OnValueInputFieldEndEdit(string value)
		{
			if (int.TryParse(value, out var intValue) && DisplayValue != intValue)
			{
				intValue = Mathf.Clamp(intValue, minValue, maxValue);
				DisplayValue = intValue;
				Save();
			}
			else
			{
				valueInputField.SetTextWithoutNotify(DisplayValue.ToString());
			}
		}
	}
}