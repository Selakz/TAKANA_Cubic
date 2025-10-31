#nullable enable

using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;

namespace T3Framework.Preset.ColorPicker
{
	public class HexColorInputField : T3MonoBehaviour
	{
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private TMP_InputField inputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<Color, ColorDataContainer>(colorDataContainer,
				(_, _) => inputField.SetTextWithoutNotify(colorDataContainer.Property.Value.ToHexAlphaTuple())),
			new InputFieldRegistrar(inputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
				s =>
				{
					try
					{
						var color = UnityParser.ParseHexAlphaTuple(s);
						colorDataContainer.Property.Value = color;
					}
					catch
					{
						inputField.SetTextWithoutNotify(colorDataContainer.Property.Value.ToHexAlphaTuple());
					}
				})
		};
	}
}