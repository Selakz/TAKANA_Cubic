#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using MusicGame.Utility.UI;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.EditorEntry.UI
{
	[Serializable]
	public struct EditorDifficultyToggleData
	{
		public Toggle toggle;
		public TMP_Text difficultyText;
		public TMP_InputField levelInputField;
	}

	public class DifficultyToggleGroupUI : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<int, EditorDifficultyToggleData> toggles = default!;
		[SerializeField] private ToggleGroup toggleGroup = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars
		{
			get
			{
				var registrars = new List<IEventRegistrar>
				{
					new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
					{
						if (info != null) UpdateUI(info);
					})
				};

				foreach (var pair in toggles.Value)
				{
					var id = pair.Key;
					var data = pair.Value;
					Rows[id] = data;
					registrars.Add(new ToggleRegistrar(
						data.toggle, isOn => OnDifficultyToggleValueChanged(id, isOn)));
					registrars.Add(new InputFieldRegistrar(
						data.levelInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
						value => OnLevelInputFieldEndEdit(id, value)));
				}

				return registrars.ToArray();
			}
		}

		// Private
		[Inject] private EditorLevelLoader levelLoader = default!;
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;

		private Dictionary<int, EditorDifficultyToggleData>? rows;
		private readonly Dictionary<int, List<IEventRegistrar>> dynamicRegistrars = new();

		private Dictionary<int, EditorDifficultyToggleData> Rows => rows ??= new(toggles.Value);

		private DifficultyViewSwitcher? viewSwitcher;
		private DifficultyViewSwitcher? ViewSwitcher => viewSwitcher ??= GetComponent<DifficultyViewSwitcher>();

		// Defined Functions
		/// <summary> Registers and wires a dynamically created row (custom difficulty). </summary>
		public void AddToggle(int id, EditorDifficultyToggleData data)
		{
			Rows[id] = data;

			var registrars = new List<IEventRegistrar>
			{
				new ToggleRegistrar(data.toggle, isOn => OnDifficultyToggleValueChanged(id, isOn)),
				new InputFieldRegistrar(
					data.levelInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit,
					value => OnLevelInputFieldEndEdit(id, value))
			};
			foreach (var registrar in registrars) registrar.Register();
			dynamicRegistrars[id] = registrars;

			if (levelInfo.Value is { } info) UpdateRow(id, info);
		}

		public void RemoveToggle(int id)
		{
			if (dynamicRegistrars.TryGetValue(id, out var registrars))
			{
				foreach (var registrar in registrars) registrar.Unregister();
				dynamicRegistrars.Remove(id);
			}

			Rows.Remove(id);
		}

		private void UpdateUI(LevelInfo levelInfo)
		{
			foreach (var id in new List<int>(Rows.Keys)) UpdateRow(id, levelInfo);

			if (levelInfo.Difficulty != 0 && levelInfo.Preference is EditorPreference preference)
				preference.Difficulty = levelInfo.Difficulty;

			ViewSwitcher?.ApplyForDifficulty(levelInfo.SongInfo, levelInfo.Difficulty);
		}

		private void UpdateRow(int id, LevelInfo levelInfo)
		{
			var data = Rows[id];
			var difficultyInfo = levelInfo.SongInfo.Difficulties.GetValueOrDefault(id);
			var isCurrent = levelInfo.Difficulty == id;

			if (data.toggle != null)
			{
				data.toggle.group = toggleGroup;
				data.toggle.interactable = true;
				data.toggle.SetIsOnWithoutNotify(isCurrent);
			}

			if (data.difficultyText != null)
				data.difficultyText.fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;

			if (data.levelInputField != null)
			{
				data.levelInputField.interactable = true;
				data.levelInputField.SetTextWithoutNotify(difficultyInfo?.LevelDisplay ?? "00");
			}
		}

		// Event Handlers
		private void OnDifficultyToggleValueChanged(int id, bool isOn)
		{
			if (Rows.TryGetValue(id, out var data) && data.difficultyText != null)
				data.difficultyText.fontStyle = isOn ? FontStyles.Bold : FontStyles.Normal;

			var info = levelInfo.Value;
			if (isOn && info != null && info.Difficulty != id)
				levelLoader.LoadLevel(Path.GetDirectoryName(info.LevelPath)!, id);
		}

		private void OnLevelInputFieldEndEdit(int id, string level)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo == null) return;

			var difficulties = info.SongInfo.Difficulties;
			if (!difficulties.TryGetValue(id, out var difficultyInfo))
			{
				difficultyInfo = new DifficultyInfo();
				difficulties[id] = difficultyInfo;
			}

			difficultyInfo.LevelDisplay = level;
			if (Rows.TryGetValue(id, out var data) && data.levelInputField != null)
				data.levelInputField.SetTextWithoutNotify(level);
		}
	}
}