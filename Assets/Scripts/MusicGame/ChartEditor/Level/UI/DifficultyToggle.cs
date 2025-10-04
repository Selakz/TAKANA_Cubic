#nullable enable

using System.IO;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class DifficultyToggle : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private int difficulty;
		[SerializeField] private EditorLevelLoader levelLoader = default!;
		[SerializeField] private Toggle difficultyToggle = default!;
		[SerializeField] private TMP_Text difficultyText = default!;
		[SerializeField] private TMP_InputField levelInputField = default!;

		public string Level
		{
			set
			{
				var difficulties = LevelManager.Instance.LevelInfo.SongInfo.Difficulties;
				difficulties.AddIf(difficulty, new(), !difficulties.ContainsKey(difficulty));
				difficulties[difficulty].LevelDisplay = value;
				levelInputField.text = value;
			}
		}

		// Private

		// Static

		// Defined Functions

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			difficultyToggle.interactable = true;
			levelInputField.interactable = true;
			difficultyText.fontStyle = levelInfo.Difficulty == difficulty ? FontStyles.Bold : FontStyles.Normal;
			levelInputField.text = levelInfo.SongInfo.Difficulties.TryGetValue(difficulty, out var difficultyInfo)
				? difficultyInfo.LevelDisplay
				: "00";
		}

		private void OnDifficultyToggleValueChanged(bool isOn)
		{
			if (isOn)
			{
				difficultyText.fontStyle = FontStyles.Bold;
				levelLoader.LoadLevel(Path.GetDirectoryName(LevelManager.Instance.LevelInfo.LevelPath), difficulty);
				if (LevelManager.Instance.LevelInfo.Preference is EditorPreference preference)
				{
					preference.Difficulty = difficulty;
				}
			}
			else
			{
				difficultyText.fontStyle = FontStyles.Normal;
			}
		}

		private void OnLevelInputFieldEndEdit(string level)
		{
			Level = level;
		}

		// System Functions
		void Awake()
		{
			difficultyToggle.onValueChanged.AddListener(OnDifficultyToggleValueChanged);
			levelInputField.onEndEdit.AddListener(OnLevelInputFieldEndEdit);
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