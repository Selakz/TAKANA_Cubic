#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Localization;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SongInfoInputField : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField musicNameInputField = default!;
		[SerializeField] private TMP_InputField composerInputField = default!;
		[SerializeField] private TMP_InputField charterInputField = default!;
		[SerializeField] private TMP_InputField illustratorInputField = default!;
		[SerializeField] private TMP_InputField bpmDisplayInputField = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				if (info != null) UpdateUI(info);
			}),
			new InputFieldRegistrar(
				musicNameInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnMusicNameInputFieldEndEdit),
			new InputFieldRegistrar(
				composerInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnComposerInputFieldEndEdit),
			new InputFieldRegistrar(
				charterInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnCharterInputFieldEndEdit),
			new InputFieldRegistrar(
				illustratorInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnIllustratorInputFieldEndEdit),
			new InputFieldRegistrar(
				bpmDisplayInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, OnBpmDisplayInputFieldEndEdit)
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		private void UpdateUI(LevelInfo levelInfo)
		{
			musicNameInputField.SetTextWithoutNotify(levelInfo.SongInfo.Title[Language.English]);
			composerInputField.SetTextWithoutNotify(levelInfo.SongInfo.Composer[Language.English]);
			var difficulties = levelInfo.SongInfo.Difficulties;
			difficulties.AddIf(levelInfo.Difficulty, new(), !difficulties.ContainsKey(levelInfo.Difficulty));
			charterInputField.SetTextWithoutNotify(difficulties[levelInfo.Difficulty].Charter[Language.English]);
			illustratorInputField.SetTextWithoutNotify(levelInfo.SongInfo.Illustrator[Language.English]);
			bpmDisplayInputField.SetTextWithoutNotify(levelInfo.SongInfo.BpmDisplay);
		}

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelInfo = levelInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnMusicNameInputFieldEndEdit(string content)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo != null)
			{
				info.SongInfo.Title[Language.English] = content;
			}
		}

		private void OnComposerInputFieldEndEdit(string content)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo != null)
			{
				info.SongInfo.Composer[Language.English] = content;
			}
		}

		private void OnCharterInputFieldEndEdit(string content)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo != null)
			{
				var difficulty = info.Difficulty;
				var difficulties = info.SongInfo.Difficulties;
				difficulties.AddIf(difficulty, new(), !difficulties.ContainsKey(difficulty));
				difficulties[difficulty].Charter[Language.English] = content;
			}
		}

		private void OnIllustratorInputFieldEndEdit(string content)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo != null)
			{
				info.SongInfo.Illustrator[Language.English] = content;
			}
		}

		private void OnBpmDisplayInputFieldEndEdit(string content)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo != null)
			{
				info.SongInfo.BpmDisplay = content;
			}
		}
	}
}