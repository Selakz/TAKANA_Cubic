#nullable enable

using System.ComponentModel;
using System.Reflection;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class T3TimeSettingItem : SingleValueSettingItem<T3Time>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField valueInputField = default!;

		protected override void InitializeSucceed()
		{
			base.InitializeSucceed();
			valueInputField.text = DisplayValue.ToString();

			var minValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MinValueAttribute>();
			if (minValueAttribute is not null) minValue = Mathf.RoundToInt(minValueAttribute.MinValue);
			var maxValueAttribute = TargetPropertyInfo!.GetCustomAttribute<MaxValueAttribute>();
			if (maxValueAttribute is not null) maxValue = Mathf.RoundToInt(maxValueAttribute.MaxValue);
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			valueInputField.text = DisplayValue.ToString();
		}

		// Private
		private T3Time minValue = T3Time.MinValue;
		private T3Time maxValue = T3Time.MaxValue;

		// Event Handlers
		private void OnValueInputFieldEndEdit(string value)
		{
			if (int.TryParse(value, out var intValue) && DisplayValue != intValue)
			{
				T3Time time = Mathf.Clamp(intValue, minValue, maxValue);
				DisplayValue = time;
				Save();
			}
			else
			{
				valueInputField.SetTextWithoutNotify(DisplayValue.ToString());
			}
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			valueInputField.onEndEdit.AddListener(OnValueInputFieldEndEdit);
		}

		protected override void OnDisable()
		{
			valueInputField.onEndEdit.RemoveListener(OnValueInputFieldEndEdit);
		}
	}
}