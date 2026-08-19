#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using EditorPlugin.PluginSystem;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EditorPlugin.EditorIntegration.UI
{
	public class EditPluginParamContent : T3MonoBehaviour
	{
		public NotifiableProperty<object> Value => valueProperty ??=
			new NotifiableProperty<object>(ApplyClamp(PluginParam.GetDefaultValue(type)))
			{
				Clamp = ApplyClamp
			};

		[field: SerializeField]
		public TMP_Text NameText { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField ValueInputField { get; set; } = default!;

		[field: SerializeField]
		public TMP_Dropdown ValueDropdown { get; set; } = default!;

		[field: SerializeField]
		public Toggle ValueToggle { get; set; } = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputFieldRegistrar(ValueInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnInputEndEdit),
			new DropdownRegistrar(ValueDropdown, OnDropdownChanged),
			new ToggleRegistrar(ValueToggle, OnToggleChanged),
			new PropertyRegistrar<object>(Value, RefreshDisplay)
		};

		// Private
		private Type type = typeof(string);
		private IReadOnlyList<OptionsMeta.Option>? options;
		private ClampMeta? clampMeta;
		private FormatMeta? formatMeta;
		private NotifiableProperty<object>? valueProperty;

		public void UpdateUI(Type type, IReadOnlyCollection<IParamMeta> metas)
		{
			options = metas.OfType<OptionsMeta>().FirstOrDefault()?.Options;
			clampMeta = metas.OfType<ClampMeta>().FirstOrDefault();
			formatMeta = metas.OfType<FormatMeta>().FirstOrDefault();

			if (type == typeof(bool))
			{
				ValueInputField.gameObject.SetActive(false);
				ValueDropdown.gameObject.SetActive(false);
				ValueToggle.gameObject.SetActive(true);
			}
			else if (options is not null)
			{
				ValueDropdown.SetOptions(options.ToList(), option => option.Text);
				ValueInputField.gameObject.SetActive(false);
				ValueDropdown.gameObject.SetActive(true);
				ValueToggle.gameObject.SetActive(false);
			}
			else
			{
				ValueInputField.gameObject.SetActive(true);
				ValueDropdown.gameObject.SetActive(false);
				ValueToggle.gameObject.SetActive(false);
			}

			if (metas.OfType<TimingMeta>().Any())
			{
				if (ValueInputField.gameObject.GetComponent<TimingInputField>() is null)
					ValueInputField.gameObject.AddComponent<TimingInputField>();
			}
			else
			{
				if (ValueInputField.gameObject.GetComponent<TimingInputField>() is { } timingInputField)
					Destroy(timingInputField);
			}

			if (this.type != type) Value.Value = ApplyClamp(PluginParam.GetDefaultValue(type));
			this.type = type;
			RefreshDisplay();
		}

		private void OnInputEndEdit(string text)
		{
			if (TryParse(text, out var value))
			{
				Value.Value = ApplyClamp(value);
				Value.AddUpNotify();
			}
			else
			{
				RefreshDisplay();
			}
		}

		private void OnDropdownChanged(int index)
		{
			if (options is not null && index >= 0 && index < options.Count)
			{
				Value.Value = options[index].Value;
			}
		}

		private void OnToggleChanged(bool value)
		{
			Value.Value = value;
		}

		private void RefreshDisplay()
		{
			if (type == typeof(bool))
			{
				if (Value.Value is bool boolValue) ValueToggle.SetIsOnWithoutNotify(boolValue);
				return;
			}

			if (options is not null)
			{
				int index = -1;
				for (int i = 0; i < options.Count; i++)
				{
					if (Equals(options[i].Value, Value.Value))
					{
						index = i;
						break;
					}
				}

				ValueDropdown.SetValueWithoutNotify(Mathf.Max(0, index));
			}
			else
			{
				ValueInputField.SetTextWithoutNotify(FormatValue(Value.Value));
			}
		}

		private bool TryParse(string text, out object value)
		{
			value = text;
			if (type == typeof(int))
			{
				if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
				{
					value = intValue;
					return true;
				}
			}
			else if (type == typeof(float))
			{
				if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
				{
					value = floatValue;
					return true;
				}
			}
			else
			{
				return true;
			}

			return false;
		}

		private object ApplyClamp(object value)
		{
			if (clampMeta is null) return value;
			if (value is int intValue)
			{
				int min = clampMeta.Min is { } minValue ? (int)Math.Round(minValue) : int.MinValue;
				int max = clampMeta.Max is { } maxValue ? (int)Math.Round(maxValue) : int.MaxValue;
				return Mathf.Clamp(intValue, min, max);
			}

			if (value is float floatValue)
			{
				float min = clampMeta.Min is { } minValue ? (float)minValue : float.MinValue;
				float max = clampMeta.Max is { } maxValue ? (float)maxValue : float.MaxValue;
				return Mathf.Clamp(floatValue, min, max);
			}

			return value;
		}

		private string FormatValue(object value)
		{
			if (formatMeta is not null && value is IFormattable formattable)
			{
				return formattable.ToString(formatMeta.Format, CultureInfo.InvariantCulture);
			}

			return value.ToString();
		}
	}
}