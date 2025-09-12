#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Localization;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SongInfoInputField : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField musicNameInputField = default!;
		[SerializeField] private TMP_InputField composerInputField = default!;
		[SerializeField] private TMP_InputField charterInputField = default!;
		[SerializeField] private TMP_InputField illustratorInputField = default!;
		[SerializeField] private TMP_InputField bpmDisplayInputField = default!;

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			musicNameInputField.interactable = true;
			musicNameInputField.SetTextWithoutNotify(levelInfo.SongInfo.Title[Language.English]);
			composerInputField.interactable = true;
			composerInputField.SetTextWithoutNotify(levelInfo.SongInfo.Composer[Language.English]);
			var difficulties = levelInfo.SongInfo.Difficulties;
			difficulties.AddIf(levelInfo.Difficulty, new(), !difficulties.ContainsKey(levelInfo.Difficulty));
			charterInputField.interactable = true;
			charterInputField.SetTextWithoutNotify(
				levelInfo.SongInfo.Difficulties[levelInfo.Difficulty].Charter[Language.English]);
			illustratorInputField.interactable = true;
			illustratorInputField.SetTextWithoutNotify(levelInfo.SongInfo.Illustrator[Language.English]);
			bpmDisplayInputField.interactable = true;
			bpmDisplayInputField.SetTextWithoutNotify(levelInfo.SongInfo.BpmDisplay);
		}

		private void OnMusicNameInputFieldEndEdit(string content)
		{
			LevelManager.Instance.LevelInfo.SongInfo.Title[Language.English] = content;
		}

		private void OnComposerInputFieldEndEdit(string content)
		{
			LevelManager.Instance.LevelInfo.SongInfo.Composer[Language.English] = content;
		}

		private void OnCharterInputFieldEndEdit(string content)
		{
			var difficulty = LevelManager.Instance.LevelInfo.Difficulty;
			var difficulties = LevelManager.Instance.LevelInfo.SongInfo.Difficulties;
			difficulties.AddIf(difficulty, new(), !difficulties.ContainsKey(difficulty));
			difficulties[difficulty].Charter[Language.English] = content;
		}

		private void OnIllustratorInputFieldEndEdit(string content)
		{
			LevelManager.Instance.LevelInfo.SongInfo.Illustrator[Language.English] = content;
		}

		private void OnBpmDisplayInputFieldEndEdit(string content)
		{
			LevelManager.Instance.LevelInfo.SongInfo.BpmDisplay = content;
		}

		// System Functions
		void Awake()
		{
			musicNameInputField.onEndEdit.AddListener(OnMusicNameInputFieldEndEdit);
			composerInputField.onEndEdit.AddListener(OnComposerInputFieldEndEdit);
			charterInputField.onEndEdit.AddListener(OnCharterInputFieldEndEdit);
			illustratorInputField.onEndEdit.AddListener(OnIllustratorInputFieldEndEdit);
			bpmDisplayInputField.onEndEdit.AddListener(OnBpmDisplayInputFieldEndEdit);
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