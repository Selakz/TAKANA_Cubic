#nullable enable

using System.ComponentModel;
using System.Reflection;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.Utility.Setting
{
	public class ColorSettingItem : SingleValueSettingItem<Color>
	{
		// Serializable and Public
		[SerializeField] private TMP_Text descriptionText = default!;
		[SerializeField] private ColorDataContainer colorDataContainer = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<Color, ColorDataContainer>(colorDataContainer, OnColorChanged)
		};

		protected override void InitializeSucceed()
		{
			colorDataContainer.Property.Value = DisplayValue;

			var attribute = TargetPropertyInfo!.GetCustomAttribute<DescriptionAttribute>();
			descriptionText.text = attribute is null ? string.Empty : attribute.Description;
		}

		protected override void InitializeFail()
		{
			descriptionText.text = $"Error fetching setting {FullClassName}.{PropertyName}";
		}

		protected override void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
		{
			colorDataContainer.Property.Value = DisplayValue;
		}

		// Event Handlers
		private void OnColorChanged(object sender, PropertyChangedEventArgs e)
		{
			if (colorDataContainer.Property.Value == DisplayValue) return;
			DisplayValue = colorDataContainer.Property.Value;
			Save();
		}
	}
}