#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class TrackOpacitySlider : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Slider trackOpacitySlider = default!;
		[SerializeField] private TMP_Text trackOpacityText = default!;

		// Private
		private int TrackOpacityPercent
		{
			set
			{
				ISingletonSetting<TrackLayerSetting>.Instance.SelectLayerOpacityRatio = value / 100f;
				ISingletonSetting<TrackLayerSetting>.SaveInstance();
				trackOpacitySlider.value = value;
				trackOpacityText.text = value.ToString();
			}
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			trackOpacitySlider.interactable = true;
			var alpha = ISingletonSetting<TrackLayerSetting>.Instance.SelectLayerOpacityRatio;
			trackOpacitySlider.value = alpha * 100;
			trackOpacityText.text = Mathf.RoundToInt(alpha * 100).ToString();
		}

		private void OnTrackOpacitySliderValueChanged(float value)
		{
			TrackOpacityPercent = Mathf.RoundToInt(value);
		}

		// System Functions
		void Awake()
		{
			trackOpacitySlider.onValueChanged.AddListener(OnTrackOpacitySliderValueChanged);
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}