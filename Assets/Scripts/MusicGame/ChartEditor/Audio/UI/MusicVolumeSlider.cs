#nullable enable

using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class MusicVolumeSlider : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Slider musicVolumeSlider = default!;
		[SerializeField] private TMP_Text musicVolumeText = default!;

		// Private
		private int MusicVolumePercent
		{
			set
			{
				LevelManager.Instance.Music.Volume = value / 100f;
				musicVolumeSlider.value = value;
				musicVolumeText.text = value.ToString();
			}
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			musicVolumeSlider.interactable = true;
			if (levelInfo.Preference is EditorPreference preference)
			{
				MusicVolumePercent = preference.MusicVolumePercent;
			}
		}

		private void OnMusicVolumeSliderValueChanged(float value)
		{
			MusicVolumePercent = Mathf.RoundToInt(value);
		}

		// System Functions
		void Awake()
		{
			musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderValueChanged);
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