#nullable enable

using System.IO;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Modifier;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.EditorEntry
{
	public class NewProjectPanel : HierarchySystem<NewProjectPanel>
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField nameInputField = default!;
		[SerializeField] private UrlInputField directoryInputField = default!;
		[SerializeField] private I18NTextBlock directoryHintText = default!;
		[SerializeField] private TMP_InputField charterIdInputField = default!;
		[SerializeField] private TMP_InputField songIdInputField = default!;
		[SerializeField] private UrlInputField audioSourceInputField = default!;
		[SerializeField] private UrlInputField coverInputField = default!;
		[SerializeField] private Button createButton = default!;

		// Private
		private EditorLevelLoader levelLoader = default!;

		private readonly NotifiableProperty<bool> isValid = new(true);
		private BoolModifier? validModifier;

		private BoolModifier ValidModifier => validModifier ??= new(
			() => isValid.Value, value => isValid.Value = value);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(isValid, value => createButton.interactable = value),
			new InputFieldRegistrar(nameInputField, InputFieldRegistrar.RegisterTarget.OnValueChanged,
				value =>
				{
					ValidModifier.Register(BoolMethod.And, !string.IsNullOrWhiteSpace(value), 0);
					UpdateDirectoryHintText();
				}),
			new PropertyRegistrar<string>(directoryInputField.Path,
				() =>
				{
					ValidModifier.Register(BoolMethod.And, directoryInputField.IsPathValid, 1);
					UpdateDirectoryHintText();
				}),
			new InputFieldRegistrar(charterIdInputField, InputFieldRegistrar.RegisterTarget.OnValueChanged,
				value => ValidModifier.Register(BoolMethod.And, !string.IsNullOrWhiteSpace(value), 2)),
			new InputFieldRegistrar(songIdInputField, InputFieldRegistrar.RegisterTarget.OnValueChanged,
				value => ValidModifier.Register(BoolMethod.And, !string.IsNullOrWhiteSpace(value), 3)),
			new PropertyRegistrar<string>(audioSourceInputField.Path,
				() => ValidModifier.Register(BoolMethod.And, audioSourceInputField.IsPathValid, 4)),
			new ButtonRegistrar(createButton, () =>
			{
				if (!isValid.Value) return;

				var directory = directoryInputField.InputField.text;
				var charterId = charterIdInputField.text;
				var songId = songIdInputField.text;
				var audioSourcePath = audioSourceInputField.InputField.text;
				var audioSourceName = $"music{Path.GetExtension(audioSourcePath)}";

				ISingletonSetting<EditorSetting>.Instance.LastProjectDirectory.Value = directory;
				ISingletonSetting<EditorSetting>.Instance.LastCharterId.Value = charterId;
				ISingletonSetting<EditorSetting>.SaveInstance();

				directory = Path.Combine(directory, nameInputField.text);
				Directory.CreateDirectory(directory);
				File.Copy(audioSourcePath, Path.Combine(directory, audioSourceName), true);
				T3ProjSetting projSetting = new() { MusicFileName = audioSourceName };
				SongInfo songInfo = new() { Id = $"{charterId}.{songId}" };
				ISetting<SongInfo>.Save(songInfo, Path.Combine(directory, projSetting.SongInfoFileName));

				if (coverInputField.IsPathValid)
				{
					var coverPath = coverInputField.InputField.text;
					var coverName = $"cover{Path.GetExtension(coverPath)}";
					File.Copy(coverPath, Path.Combine(directory, coverName), true);
					projSetting.CoverFileName = coverName;
				}

				ISetting<T3ProjSetting>.Save(projSetting, Path.Combine(directory, $"{songId}.t3proj"));
				levelLoader.LoadLevel(directory);
			})
		};

		// Constructor
		[Inject]
		private void Construct(EditorLevelLoader levelLoader)
		{
			this.levelLoader = levelLoader;
		}

		// Defined Functions
		public void ClearPanel()
		{
			nameInputField.text = string.Empty;
			directoryInputField.InputField.text = ISingletonSetting<EditorSetting>.Instance.LastProjectDirectory;
			charterIdInputField.text = ISingletonSetting<EditorSetting>.Instance.LastCharterId;
			songIdInputField.text = string.Empty;
			audioSourceInputField.InputField.text = string.Empty;
			coverInputField.InputField.text = string.Empty;
		}

		private void UpdateDirectoryHintText()
		{
			if (!directoryInputField.IsPathValid)
			{
				directoryHintText.SetText("EditorEntry_InvalidDirectory", directoryInputField.InputField.text);
			}
			else if (string.IsNullOrWhiteSpace(nameInputField.text))
			{
				directoryHintText.SetText("EditorEntry_InvalidProjectName");
			}
			else
			{
				directoryHintText.SetText("EditorEntry_TargetDirectory",
					Path.Combine(directoryInputField.InputField.text, nameInputField.text));
			}
		}

		// System Functions
		void Start()
		{
			ClearPanel();
		}
	}
}