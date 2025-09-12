using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.TrackLayer.UI;
using MusicGame.Components.Chart;
using MusicGame.Components.Tracks;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLayer
{
	[RequireComponent(typeof(TrackController))]
	public class TrackLayerHandler : MonoBehaviour
	{
		// Serializable and Public
		private LayerInfo LayerInfo
		{
			get => Model == null ? TrackLayerManager.Instance.FallbackLayerInfo : Model.GetLayer();
			set => Model.SetLayer(value.Id);
		}

		// Private
		private TrackController trackController;

		private EditingTrack Model
		{
			get
			{
				var id = trackController.Model.Id;
				var component = IEditingChartManager.Instance.Chart[id];
				if (component is EditingTrack editingTrack)
				{
					return editingTrack;
				}
				else
				{
					return null;
				}
			}
		}

		// Static
		private const int LayerColorPriority = 5;

		// Defined Functions

		// Event Handlers
		private void ChartOnUpdate(ChartInfo chartInfo)
		{
			LayerAfterUpdate(LayerInfo);
		}

		private void LayerAfterUpdate(LayerInfo layerInfo)
		{
			if (layerInfo.Id != LayerInfo.Id) return;

			trackController.ColliderEnabledModifier.Register(_ => layerInfo.IsSelected, LayerColorPriority);
			trackController.ColorModifier.Register(
				color =>
				{
					float alphaRate = layerInfo.Color.a * (layerInfo.IsSelected
						? ISingletonSetting<TrackLayerSetting>.Instance.SelectLayerOpacityRatio
						: ISingletonSetting<TrackLayerSetting>.Instance.UnselectLayerOpacityRatio);
					return new Color(layerInfo.Color.r, layerInfo.Color.g, layerInfo.Color.b, color.a * alphaRate);
				},
				LayerColorPriority);
			trackController.SortingOrderModifier.Register(
				order => order + (
					EditLayerContent.MaximumPaletteCount - TrackLayerManager.Instance.GetSiblingIndex(layerInfo.Id)),
				LayerColorPriority);
		}

		private void LayerAfterRemove(LayerInfo layerInfo)
		{
			LayerAfterUpdate(LayerInfo);
		}

		// System Functions
		void Awake()
		{
			trackController = GetComponent<TrackController>();
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
			EventManager.Instance.AddListener<LayerInfo>("Layer_AfterUpdate", LayerAfterUpdate);
			EventManager.Instance.AddListener<LayerInfo>("Layer_AfterRemove", LayerAfterRemove);
			LayerAfterUpdate(LayerInfo);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<ChartInfo>("Chart_OnUpdate", ChartOnUpdate);
			EventManager.Instance.RemoveListener<LayerInfo>("Layer_AfterUpdate", LayerAfterUpdate);
			EventManager.Instance.RemoveListener<LayerInfo>("Layer_AfterRemove", LayerAfterRemove);
		}
	}
}