#nullable enable

using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class HitSoundVolumeSlider : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Slider hitSoundVolumeSlider = default!;
		[SerializeField] private TMP_Text hitSoundVolumeText = default!;

		// Private
		private int HitSoundVolumePercent
		{
			set
			{
				HitSoundManager.Instance.Volume = value / 100f;
				hitSoundVolumeSlider.value = value;
				hitSoundVolumeText.text = value.ToString();
			}
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			hitSoundVolumeSlider.interactable = true;
			HitSoundVolumePercent = ISingletonSetting<EditorSetting>.Instance.HitSoundVolumePercent;
		}

		private void OnMusicVolumeSliderValueChanged(float value)
		{
			HitSoundVolumePercent = Mathf.RoundToInt(value);
		}

		// System Functions
		void Awake()
		{
			hitSoundVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderValueChanged);
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