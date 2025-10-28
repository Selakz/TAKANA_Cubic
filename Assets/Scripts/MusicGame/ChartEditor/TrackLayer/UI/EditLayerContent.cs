using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Message;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.ListRender;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class EditLayerContent : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Toggle isSelectedToggle;
		[SerializeField] private Toggle isDecorationToggle;
		[SerializeField] private Button upLevelButton;
		[SerializeField] private Button downLevelButton;
		[SerializeField] private TMP_InputField nameInputField;
		[SerializeField] private ListRendererInt paletteRenderer;
		[SerializeField] private Toggle colorPicker;
		[SerializeField] private GameObject colorPalette;
		[SerializeField] private Image colorSample;
		[SerializeField] private DoubleClickButton removeButton;

		public LayerInfo LayerInfo
		{
			get => layerInfo;
			set
			{
				layerInfo = value;
				nameInputField.SetTextWithoutNotify(layerInfo.Name);
				isSelectedToggle.SetIsOnWithoutNotify(layerInfo.IsSelected);
				isDecorationToggle.SetIsOnWithoutNotify(layerInfo.IsDecoration);
				colorPicker.SetIsOnWithoutNotify(false);
				colorPalette.SetActive(false);
				colorSample.color = layerInfo.Color;
				if (layerInfo.Id == ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId)
				{
					nameInputField.interactable = false;
					removeButton.Button.interactable = false;
				}

				transform.localScale = Vector3.one;
			}
		}

		public int ListOrder { get; set; }

		// Private
		private LayerInfo layerInfo;

		// Static
		public const int MaximumPaletteCount = 16;

		// Defined Functions

		// Event Handlers
		private void InvokeUpdateEvent()
		{
			EventManager.Instance.Invoke("Layer_AfterUpdate", layerInfo);
		}

		private void InvokeUpdateEvent(int layerId)
		{
			var givenLayerInfo = TrackLayerManager.Instance.TryGetLayer(layerId, out var gotLayerInfo)
				? gotLayerInfo
				: TrackLayerManager.Instance.FallbackLayerInfo;
			EventManager.Instance.Invoke("Layer_AfterUpdate", givenLayerInfo);
		}

		private void OnIsSelectedToggleValueChanged(bool isSelected)
		{
			layerInfo.IsSelected = isSelected;
			InvokeUpdateEvent();
			TrackLayerManager.Instance.SaveLayersToChart();
		}

		private void OnIsDecorationToggleValueChanged(bool isDecoration)
		{
			if (isDecoration)
			{
				foreach (var component in IEditingChartManager.Instance.Chart)
				{
					if (component is not EditingTrack editingTrack ||
					    editingTrack.Properties.Get("layer",
						    TrackLayerManager.Instance.FallbackLayerInfo.Id) != LayerInfo.Id)
						continue;
					var children = IEditingChartManager.Instance.GetChildrenComponents(component.Id);
					if (children.Any(c => c is EditingNote))
					{
						HeaderMessage.Show($"无法设为装饰图层，轨道{component.Id}上存在Note", HeaderMessage.MessageType.Info);
						isDecorationToggle.SetIsOnWithoutNotify(false);
						layerInfo.IsDecoration = false;
						return;
					}
				}

				layerInfo.IsDecoration = true;
				HeaderMessage.Show("设置成功，该图层将无法放置Note", HeaderMessage.MessageType.Success);
			}
			else
			{
				layerInfo.IsDecoration = false;
				HeaderMessage.Show("取消装饰，该图层将可以放置Note", HeaderMessage.MessageType.Info);
			}

			InvokeUpdateEvent();
			TrackLayerManager.Instance.SaveLayersToChart();
		}

		private void OnUpLevelButtonClicked()
		{
			int previousListOrder = ListOrder;
			if (TrackLayerManager.Instance.ListRenderer.SwapUp(previousListOrder, out var swappedOrder))
			{
				if (TrackLayerManager.Instance.ListRenderer.TryGet<EditLayerContent>(ListOrder, out var swappedContent))
				{
					swappedContent.ListOrder = ListOrder;
				}

				ListOrder = swappedOrder;
				InvokeUpdateEvent();
				InvokeUpdateEvent(previousListOrder);
				TrackLayerManager.Instance.SaveLayersToChart();
			}
		}

		private void OnDownLevelButtonClicked()
		{
			int previousListOrder = ListOrder;
			if (TrackLayerManager.Instance.ListRenderer.SwapDown(previousListOrder, out var swappedOrder))
			{
				if (TrackLayerManager.Instance.ListRenderer.TryGet<EditLayerContent>(ListOrder, out var swappedContent))
				{
					swappedContent.ListOrder = ListOrder;
				}

				ListOrder = swappedOrder;
				InvokeUpdateEvent();
				InvokeUpdateEvent(previousListOrder);
				TrackLayerManager.Instance.SaveLayersToChart();
			}
		}

		private void OnNameInputFieldEndEdit(string content)
		{
			if (string.IsNullOrEmpty(content) ||
			    layerInfo.Name == content ||
			    layerInfo.Id == ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId)
			{
				nameInputField.SetTextWithoutNotify(layerInfo.Name);
				return;
			}

			layerInfo.Name = content;
			InvokeUpdateEvent();
			TrackLayerManager.Instance.SaveLayersToChart();
		}

		private void OnColorPickerValueChanged(bool isOn)
		{
			colorPalette.SetActive(isOn);
			TrackLayerManager.Instance.ListRenderer.RebuildLayout();
		}

		private void OnPaletteButtonClicked(object sender, Color color)
		{
			if (layerInfo.Color == color) return;
			layerInfo.Color = color;
			colorSample.color = color;
			InvokeUpdateEvent();
			TrackLayerManager.Instance.SaveLayersToChart();
		}

		private void OnRemoveButtonFirstClicked()
		{
			if (layerInfo.Id == ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId)
			{
				HeaderMessage.Show("不能删除默认图层", HeaderMessage.MessageType.Info);
				return;
			}

			int attachedLayerCount = 0;
			foreach (var component in IEditingChartManager.Instance.Chart)
			{
				if (component is not EditingTrack editingTrack ||
				    editingTrack.Properties.Get("layer",
					    TrackLayerManager.Instance.FallbackLayerInfo.Id) != LayerInfo.Id)
					continue;
				attachedLayerCount++;
			}

			HeaderMessage.Show($"该图层上有{attachedLayerCount}个轨道；再次点击以删除", HeaderMessage.MessageType.Info);
		}

		private void OnRemoveButtonSecondClicked()
		{
			if (layerInfo.Id == ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId)
			{
				HeaderMessage.Show("不能删除默认图层", HeaderMessage.MessageType.Info);
				return;
			}

			TrackLayerManager.Instance.ListRenderer.Remove(ListOrder);
			EventManager.Instance.Invoke("Layer_AfterRemove", layerInfo);
			HeaderMessage.Show("删除成功！", HeaderMessage.MessageType.Success);
			TrackLayerManager.Instance.SaveLayersToChart();
		}

		// System Functions
		void Awake()
		{
			isSelectedToggle.onValueChanged.AddListener(OnIsSelectedToggleValueChanged);
			isDecorationToggle.onValueChanged.AddListener(OnIsDecorationToggleValueChanged);
			upLevelButton.onClick.AddListener(OnUpLevelButtonClicked);
			downLevelButton.onClick.AddListener(OnDownLevelButtonClicked);
			nameInputField.onEndEdit.AddListener(OnNameInputFieldEndEdit);
			colorPicker.onValueChanged.AddListener(OnColorPickerValueChanged);
			removeButton.OnFirstClick += OnRemoveButtonFirstClicked;
			removeButton.OnSecondClick += OnRemoveButtonSecondClicked;
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
				button.PaletteColor = colorDefinition;
				button.OnColorClicked += OnPaletteButtonClicked;
			}
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