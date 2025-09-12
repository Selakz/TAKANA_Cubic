using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SpeedSlider : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Slider speedSlider;
		[SerializeField] private TMP_Text speedText;

		// Private
		private float Speed
		{
			set
			{
				LevelManager.Instance.LevelSpeed.Speed = value;
				speedSlider.value = value * 10;
				speedText.text = value.ToString("0.0");
				// TODO: Move to LevelManager
				EventManager.Instance.Invoke("Level_OnSpeedUpdate", LevelManager.Instance.LevelSpeed.SpeedRate);
			}
		}

		// Static

		// Defined Functions

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			speedSlider.interactable = true;
			if (levelInfo.Preference is EditorPreference preference)
			{
				Speed = preference.Speed;
			}
		}

		private void OnSpeedSliderValueChanged(float value)
		{
			float speed = value / 10;
			Speed = speed;
			if (LevelManager.Instance.LevelInfo.Preference is EditorPreference preference)
			{
				preference.Speed = speed;
			}
		}

		// System Functions
		void Awake()
		{
			speedSlider.onValueChanged.AddListener(OnSpeedSliderValueChanged);
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