#nullable enable

using System;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.Setting
{
	public class ColorListItemSettingItem : T3MonoBehaviour, IListItemSettingItem<Color?>
	{
		// Serializable and Public
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private Button addButton = default!;
		[SerializeField] private Button removeButton = default!;

		public Color? Data
		{
			get => currentColor;
			set => colorDataContainer.Property.Value = value ?? colorDataContainer.InitialValue;
		}

		public event Action<Color?, Color?>? OnListContentChanged;

		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<Color, ColorDataContainer>(colorDataContainer,
				(_, _) =>
				{
					currentColor = colorDataContainer.Property.Value;
					OnListContentChanged?.Invoke(previousColor, currentColor);
					previousColor = currentColor;
				})
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(addButton, () => OnListContentChanged?.Invoke(null, currentColor)),
			new ButtonRegistrar(removeButton, () => OnListContentChanged?.Invoke(currentColor, null))
		};

		// Private
		private Color? previousColor;
		private Color? currentColor;
	}
}