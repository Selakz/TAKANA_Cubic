#nullable enable

using System;
using T3Framework.Preset.DataContainers;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace T3Framework.Preset.ColorPicker
{
	public class ColorPickingManager : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private ColorDataContainer colorDataContainer = default!;
		[SerializeField] private Button confirmButton = default!;
		[SerializeField] private Button cancelButton = default!;
		[SerializeField] private GameObject colorPickerPanel = default!;

		public static ColorPickingManager Instance { get; private set; } = default!;

		private event Action<Color>? OnCloseDialog;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(confirmButton, OnConfirmButtonClick),
			new ButtonRegistrar(cancelButton, OnCancelButtonClick)
		};

		// Defined Functions
		// TODO: use UniTask to refactor this method to "async"
		public void PickColor(Color initialColor, Action<Color> callback)
		{
			colorDataContainer.Property.Value = initialColor;
			OnCloseDialog += callback;
			colorPickerPanel.SetActive(true);
		}

		// Event Handlers
		private void OnConfirmButtonClick()
		{
			colorPickerPanel.SetActive(false);
			OnCloseDialog?.Invoke(colorDataContainer.Property.Value);
			OnCloseDialog = null;
		}

		private void OnCancelButtonClick()
		{
			colorPickerPanel.SetActive(false);
			OnCloseDialog = null;
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			Instance = this;
		}
	}
}