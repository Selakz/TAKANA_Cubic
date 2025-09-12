using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.TrackLayer.UI;
using MusicGame.Components.Notes;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.ListRender;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer
{
	/// <summary>
	/// Guarantees a layer with id 0 and a fixed name.
	/// </summary>
	public class TrackLayerManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private ListRendererInt listRenderer;
		[SerializeField] private TMP_InputField newLayerNameInputField;
		[SerializeField] private Button addNewLayerButton;

		public static TrackLayerManager Instance { get; private set; }

		public ListRendererInt ListRenderer => listRenderer;

		public LayerInfo FallbackLayerInfo
		{
			get
			{
				foreach (var go in ListRenderer.Values)
				{
					if (go.TryGetComponent<EditLayerContent>(out var content) &&
					    content.LayerInfo.Id == ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId)
					{
						return content.LayerInfo;
					}
				}

				throw new NullReferenceException("No default layer found");
			}
		}

		// Private

		// Static
		private static int newLayerId = 0;

		// Defined Functions
		/// <summary> If not found, return <see cref="FallbackLayerInfo"/> </summary>
		public bool TryGetLayer(int id, out LayerInfo layerInfo)
		{
			foreach (var go in ListRenderer.Values)
			{
				if (go.TryGetComponent<EditLayerContent>(out var content) && content.LayerInfo.Id == id)
				{
					layerInfo = content.LayerInfo;
					return true;
				}
			}

			layerInfo = FallbackLayerInfo;
			return false;
		}

		public IEnumerable<LayerInfo> GetAllLayers()
		{
			foreach (var go in ListRenderer.Values)
			{
				if (go.TryGetComponent<EditLayerContent>(out var content))
				{
					yield return content.LayerInfo;
				}
			}
		}

		public int GetSiblingIndex(int layerId)
		{
			int result = -1;
			foreach (var pair in ListRenderer)
			{
				if (pair.Value.TryGetComponent<EditLayerContent>(out var content) && content.LayerInfo.Id == layerId)
				{
					result = ListRenderer.GetSiblingIndex(pair.Key);
					break;
				}
			}

			return result;
		}

		public void SaveLayersToChart()
		{
			var chart = LevelManager.Instance.LevelInfo.Chart;
			chart.Properties.Set(new JArray(GetAllLayers().Select(layer => layer.GetSerializationToken())),
				"editorconfig", "layers");
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			ListRenderer.Clear();
			var chart = levelInfo.Chart;
			var layers = chart.Properties.Get("editorconfig", new JObject()).Get("layers");
			bool hasDefaultLayer = false;
			if (layers is JArray layerList)
			{
				for (var i = 0; i < layerList.Count; i++)
				{
					var layerToken = layerList[i];
					var layer = LayerInfo.Deserialize(layerToken);
					if (layer.Id == ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId)
					{
						hasDefaultLayer = true;
						layer.Name = ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerName;
					}

					var content = listRenderer.Add<EditLayerContent>(i);
					content.ListOrder = i;
					content.LayerInfo = layer;
					newLayerId = Mathf.Max(newLayerId, layer.Id, i);
				}
			}

			if (!hasDefaultLayer)
			{
				var defaultContent =
					listRenderer.Add<EditLayerContent>(ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId);
				LayerInfo defaultLayer = new()
				{
					Id = ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId,
					Name = ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerName,
					Color = ISingletonSetting<TrackLayerSetting>.Instance.DefaultColor,
					IsDecoration = false,
					IsSelected = true
				};
				defaultContent.ListOrder = ISingletonSetting<TrackLayerSetting>.Instance.DefaultLayerId;
				defaultContent.LayerInfo = defaultLayer;
			}
		}

		private void VetoPlaceNote(VetoArg arg, INote note)
		{
			if (note.Parent is null) return;
			if (IEditingChartManager.Instance.Chart.TryGetComponent(note.Parent.Id, out var parent) &&
			    parent is EditingTrack editingTrack &&
			    editingTrack.GetLayer().IsDecoration)
			{
				arg.Veto("该轨道位于装饰图层，无法放置Note");
			}
		}

		private void OnAddNewLayerButtonPressed()
		{
			string layerName = newLayerNameInputField.text;
			if (string.IsNullOrEmpty(layerName)) return;
			newLayerNameInputField.SetTextWithoutNotify(string.Empty);
			newLayerId++;
			LayerInfo newLayer = new()
			{
				Id = newLayerId,
				Name = layerName,
				Color = ISingletonSetting<TrackLayerSetting>.Instance.DefaultColor,
				IsDecoration = false,
				IsSelected = true
			};
			var content = listRenderer.Add<EditLayerContent>(newLayerId);
			content.ListOrder = newLayerId;
			content.LayerInfo = newLayer;
			SaveLayersToChart();
			IEditingChartManager.Instance.UpdateProperties();
		}

		// System Functions
		void Awake()
		{
			addNewLayerButton.onClick.AddListener(OnAddNewLayerButtonPressed);
			listRenderer.Init(new()
			{
				[typeof(EditLayerContent)] =
					new LazyPrefab("Prefabs/EditorUI/TrackLayer/EditLayerContent", "EditLayerContentPrefab_OnLoad")
			});
		}

		void OnEnable()
		{
			Instance = this;
			EventManager.Instance.AddListener<GameObject>("TrackPrefab_OnLoad",
				go => go.AddComponent<TrackLayerHandler>());
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.AddVetoListener<INote>("Edit_QueryPlaceNote", VetoPlaceNote);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<GameObject>("TrackPrefab_OnLoad",
				go => go.AddComponent<TrackLayerHandler>());
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			EventManager.Instance.RemoveVetoListener<INote>("Edit_QueryPlaceNote", VetoPlaceNote);
		}
	}
}