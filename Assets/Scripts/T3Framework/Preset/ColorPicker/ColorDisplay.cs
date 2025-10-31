#nullable enable

using System.ComponentModel;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.ColorPicker
{
	public class ColorDisplay : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private Graphic graphic = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<Color, ColorDataContainer>(colorDataContainer, OnColorChanged)
		};

		private void OnColorChanged(object sender, PropertyChangedEventArgs e)
		{
			graphic.color = colorDataContainer.Property;
		}
	}
}