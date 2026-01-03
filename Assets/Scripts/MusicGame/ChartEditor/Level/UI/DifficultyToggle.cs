#nullable enable

using System.IO;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.ChartEditor.Level.UI
{
	public class DifficultyToggle : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private int difficulty;
		[SerializeField] private Toggle difficultyToggle = default!;
		[SerializeField] private TMP_Text difficultyText = default!;
		[SerializeField] private TMP_InputField levelInputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				if (info != null) UpdateUI(info);
			}),
			new ToggleRegistrar(
				difficultyToggle, OnDifficultyToggleValueChanged),
			new InputFieldRegistrar(
				levelInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnLevelInputFieldEndEdit)
		};

		// Private
		private EditorLevelLoader levelLoader = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		private void UpdateUI(LevelInfo levelInfo)
		{
			difficultyToggle.interactable = true;
			levelInputField.interactable = true;
			difficultyText.fontStyle = levelInfo.Difficulty == difficulty ? FontStyles.Bold : FontStyles.Normal;
			levelInputField.text = levelInfo.SongInfo.Difficulties.TryGetValue(difficulty, out var difficultyInfo)
				? difficultyInfo.LevelDisplay
				: "00";
		}

		// Constructor
		[Inject]
		private void Construct(
			EditorLevelLoader levelLoader,
			NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelLoader = levelLoader;
			this.levelInfo = levelInfo;
		}

		// TODO: Currently VContainer do not support RegisterComponent with key. This inject will temporarily be done by the LifetimeScope.
		// public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnDifficultyToggleValueChanged(bool isOn)
		{
			var info = levelInfo.Value;
			if (isOn && info != null)
			{
				difficultyText.fontStyle = FontStyles.Bold;
				levelLoader.LoadLevel(Path.GetDirectoryName(info.LevelPath)!, difficulty);
				info = levelInfo.Value;
				if (info?.Preference is EditorPreference preference)
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
			var info = levelInfo.Value;
			if (info?.SongInfo != null)
			{
				var difficulties = info.SongInfo.Difficulties;
				difficulties.AddIf(difficulty, new(), !difficulties.ContainsKey(difficulty));
				difficulties[difficulty].LevelDisplay = level;
				levelInputField.text = level;
			}
		}
	}
}