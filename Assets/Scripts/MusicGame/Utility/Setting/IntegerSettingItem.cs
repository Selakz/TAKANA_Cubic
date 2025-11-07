#nullable enable

using System.ComponentModel;
using System.Reflection;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class IntegerSettingItem : SingleValueSettingItem<int>
	{
		// Serializable and Public
		[SerializeField] private TMP_Text descriptionText = default!;
		[SerializeField] private TMP_InputField valueInputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar
				(valueInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnValueInputFieldEndEdit)
		};

		protected override void InitializeSucceed()
		{
			var descriptionAttribute = TargetPropertyInfo!.GetCustomAttribute<DescriptionAttribute>();
			descriptionText.text = descriptionAttribute is null ? string.Empty : descriptionAttribute.Description;
			valueInputField.text = DisplayValue.ToString();

			var minValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MinValueAttribute>();
			if (minValueAttribute is not null) minValue = Mathf.RoundToInt(minValueAttribute.MinValue);
			var maxValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxValueAttribute>();
			if (maxValueAttribute is not null) maxValue = Mathf.RoundToInt(maxValueAttribute.MaxValue);
		}

		protected override void InitializeFail()
		{
			descriptionText.text = $"Error fetching setting {FullClassName}.{PropertyName}";
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			valueInputField.text = DisplayValue.ToString();
		}

		// Private
		private int minValue = int.MinValue;
		private int maxValue = int.MaxValue;

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