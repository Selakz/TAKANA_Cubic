#nullable enable

using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.ColorPicker
{
	public class OpenColorPickerButton : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private Button button = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button,
				() => ColorPickingManager.Instance.PickColor(colorDataContainer.Property.Value,
					color => colorDataContainer.Property.Value = color)
			)
		};
	}
}