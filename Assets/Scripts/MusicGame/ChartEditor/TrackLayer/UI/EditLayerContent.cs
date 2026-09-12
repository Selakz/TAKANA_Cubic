#nullable enable

using System.ComponentModel;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class EditLayerContent : MonoBehaviour
	{
		// Serializable and Public
		[field: SerializeField]
		public Toggle IsSelectedToggle { get; set; } = default!;

		[field: SerializeField]
		public Toggle IsDecorationToggle { get; set; } = default!;

		[field: SerializeField]
		public Button UpLevelButton { get; set; } = default!;

		[field: SerializeField]
		public Button DownLevelButton { get; set; } = default!;

		[field: SerializeField]
		public TMP_InputField NameInputField { get; set; } = default!;

		[field: SerializeField]
		public DoubleClickButton RemoveButton { get; set; } = default!;

		public NotifiableProperty<Color> PaletteColor { get; } = new(Color.black);

		[SerializeField] private PrefabObject paletteButtonPrefab = default!;
		[SerializeField] private RectTransform paletteContent = default!;
		[SerializeField] private Toggle colorPicker = default!;
		[SerializeField] private GameObject colorPalette = default!;
		[SerializeField] private Image colorSample = default!;

		// Static
		public const int MaximumPaletteCount = 16;

		// Private
		private ViewPool<BaseComponent<Color>> paletteViewPool = default!;

		// Event Handlers
		private void OnColorPickerValueChanged(bool isOn) => colorPalette.SetActive(isOn);

		private void OnPaletteButtonClicked(object sender, Color color) => PaletteColor.Value = color;

		private void ColorChanged(object sender, PropertyChangedEventArgs e)
		{
			var color = PaletteColor.Value;
			colorSample.color = color;
		}

		// System Functions
		void Awake()
		{
			paletteViewPool = new(null, paletteButtonPrefab, paletteContent);
		}

		void OnEnable()
		{
			var colorDefinitions = ISingletonSetting<TrackLayerSetting>.Instance.ColorDefinitions.Value;
			var paletteCount = Mathf.Min(colorDefinitions.Count, MaximumPaletteCount);
			for (var i = 0; i < paletteCount; i++)
			{
				var component = new BaseComponent<Color>(colorDefinitions[i]!.Value);
				if (!paletteViewPool.Add(component)) continue;
				var button = paletteViewPool[component]!.Script<PaletteButton>();
				button.transform.SetSiblingIndex(i);
				button.PaletteColor = component.Model;
				button.OnColorClicked += OnPaletteButtonClicked;
			}

			LayoutRebuilder.ForceRebuildLayoutImmediate(paletteContent);

			colorPicker.onValueChanged.AddListener(OnColorPickerValueChanged);
			colorPicker.isOn = false;
			PaletteColor.PropertyChanged += ColorChanged;
			ColorChanged(this, null!);
		}

		void OnDisable()
		{
			foreach (var component in paletteViewPool)
			{
				paletteViewPool[component]!.Script<PaletteButton>().OnColorClicked -= OnPaletteButtonClicked;
			}

			paletteViewPool.Clear();
			LayoutRebuilder.ForceRebuildLayoutImmediate(paletteContent);
		}
	}
}
