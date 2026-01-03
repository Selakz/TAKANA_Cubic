#nullable enable

using System.ComponentModel;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.ListRender;
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

		[SerializeField] private ListRendererInt paletteRenderer = default!;
		[SerializeField] private Toggle colorPicker = default!;
		[SerializeField] private GameObject colorPalette = default!;
		[SerializeField] private Image colorSample = default!;

		// Static
		public const int MaximumPaletteCount = 16;

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
			paletteRenderer.Init(new()
			{
				[typeof(PaletteButton)] = new("Prefabs/EditorUI/TrackLayer/PaletteButton", "PaletteButtonPrefab_OnLoad")
			});
		}

		void OnEnable()
		{
			var paletteCount = Mathf.Min(
				ISingletonSetting<TrackLayerSetting>.Instance.ColorDefinitions.Value.Count, MaximumPaletteCount);
			for (var i = 0; i < paletteCount; i++)
			{
				var colorDefinition = ISingletonSetting<TrackLayerSetting>.Instance.ColorDefinitions.Value[i];
				var button = paletteRenderer.Add<PaletteButton>(i);
				button.PaletteColor = colorDefinition!.Value;
				button.OnColorClicked += OnPaletteButtonClicked;
			}

			colorPicker.onValueChanged.AddListener(OnColorPickerValueChanged);
			colorPicker.isOn = false;
			PaletteColor.PropertyChanged += ColorChanged;
			ColorChanged(this, null!);
		}

		void OnDisable()
		{
			foreach (var go in paletteRenderer.Values)
			{
				if (go.TryGetComponent<PaletteButton>(out var paletteButton))
				{
					paletteButton.OnColorClicked -= OnPaletteButtonClicked;
				}
			}

			paletteRenderer.Clear();
		}
	}
}