#nullable enable

using System;
using System.ComponentModel;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;

namespace T3Framework.Preset.ColorPicker
{
	public class RgbColorInputField : T3MonoBehaviour
	{
		// Serializable and Public
		public enum Dimension
		{
			R,
			G,
			B,
			A
		}

		[SerializeField] private Dimension dimension = Dimension.R;
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private TMP_InputField inputField = default!;
		[SerializeField] private string format = "0.000";

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<Color, ColorDataContainer>(colorDataContainer, OnColorChanged),
			new InputFieldRegistrar(inputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnInputFieldEndEdit)
		};

		private void OnColorChanged(object sender, PropertyChangedEventArgs e)
		{
			var color = colorDataContainer.Property.Value;
			var value = dimension switch
			{
				Dimension.R => color.r,
				Dimension.G => color.g,
				Dimension.B => color.b,
				Dimension.A => color.a,
				_ => throw new ArgumentOutOfRangeException()
			};
			inputField.SetTextWithoutNotify(value.ToString(format));
		}

		private void OnInputFieldEndEdit(string content)
		{
			Color color = colorDataContainer.Property.Value;
			var previousValue = dimension switch
			{
				Dimension.R => color.r,
				Dimension.G => color.g,
				Dimension.B => color.b,
				Dimension.A => color.a,
				_ => throw new ArgumentOutOfRangeException()
			};

			if (float.TryParse(content, out float value) && !Mathf.Approximately(previousValue, value))
			{
				switch (dimension)
				{
					case Dimension.R:
						color.r = value;
						break;
					case Dimension.G:
						color.g = value;
						break;
					case Dimension.B:
						color.b = value;
						break;
					case Dimension.A:
						color.a = value;
						break;
				}

				colorDataContainer.Property.Value = color;
			}
		}
	}
}