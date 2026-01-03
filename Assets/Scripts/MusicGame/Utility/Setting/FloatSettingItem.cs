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
	public class FloatSettingItem : SingleValueSettingItem<float>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField valueInputField = default!;
		[SerializeField] private string valueFormat = "0.000";

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar
				(valueInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnValueInputFieldEndEdit)
		};

		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			valueInputField.text = DisplayValue.ToString(valueFormat);

			var minValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MinValueAttribute>();
			if (minValueAttribute is not null) minValue = minValueAttribute.MinValue;
			var maxValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxValueAttribute>();
			if (maxValueAttribute is not null) maxValue = maxValueAttribute.MaxValue;
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			valueInputField.text = DisplayValue.ToString(valueFormat);
		}

		// Private
		private float minValue = float.MinValue;
		private float maxValue = float.MaxValue;

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