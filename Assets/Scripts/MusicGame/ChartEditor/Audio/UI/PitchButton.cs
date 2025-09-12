using System.Collections.Generic;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class PitchButton : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Button pitchButton;
		[SerializeField] private TMP_Text speedText;

		// Private
		// TODO: Extract to setting
		private readonly List<float> speeds = new() { 1.00f, 0.75f, 0.50f, 0.25f };
		private int speedIndex = 0;

		private float Pitch
		{
			set => speedText.text = $"{Mathf.RoundToInt(value * 100)}%";
		}

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			pitchButton.interactable = true;
		}

		private void OnPitchButtonClick()
		{
			speedIndex = (speedIndex + 1) % speeds.Count;
			LevelManager.Instance.Music.Pitch = speeds[speedIndex];
		}

		// System Functions
		void Awake()
		{
			pitchButton.onClick.AddListener(OnPitchButtonClick);
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void Update()
		{
			Pitch = LevelManager.Instance.Music.Pitch;
		}
	}
}