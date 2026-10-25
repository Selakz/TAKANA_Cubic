#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Message;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.ColorPicker;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.EditorEntry.UI
{
	public class CustomDifficultyToggleSystem : HierarchySystem<CustomDifficultyToggleSystem>
	{
		// Serializable and Public
		[SerializeField] private DifficultyToggleGroupUI toggleGroup = default!;
		[SerializeField] private GameObject customTogglePrefab = default!;
		[SerializeField] private Transform toggleParent = default!;
		[SerializeField] private Button addButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
			{
				if (info != null) SyncCustomToggles(info);
			}),
			new ButtonRegistrar(addButton, AddCustomDifficulty)
		};

		// Private
		[Inject] private NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private EditorLevelSaver levelSaver = default!;
		[Inject] private EditorLevelLoader levelLoader = default!;
		[Inject] private MessageBox messageBox = default!;

		private const string DefaultCustomDifficultyName = "CHARTLAB";

		private readonly Dictionary<int, CustomDifficultyRow> customRows = new();

		// Defined Functions
		private void AddCustomDifficulty()
		{
			var info = levelInfo.Value;
			if (info?.SongInfo == null) return;

			var id = DifficultyCatalog.NextCustomId(info.SongInfo);
			info.SongInfo.Difficulties[id] = new DifficultyInfo
			{
				IsCustom = true,
				Name = DefaultCustomDifficultyName,
				Color = ISingleton<EditorSetting>.Instance.DefaultCustomDifficultyColor,
				LevelDisplay = "00"
			};

			levelSaver.SaveSettings();
			SyncCustomToggles(info);
		}

		private void OnNameChanged(int id, string value)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo == null) return;
			if (!info.SongInfo.Difficulties.TryGetValue(id, out var difficultyInfo)) return;

			difficultyInfo.Name = value;
			levelSaver.SaveSettings();
			if (customRows.TryGetValue(id, out var row)) row.nameInputField.SetTextWithoutNotify(value);
		}

		private void OnPickColor(int id)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo == null) return;
			if (!info.SongInfo.Difficulties.TryGetValue(id, out var difficultyInfo)) return;

			ColorPickingManager.Instance.PickColor(difficultyInfo.Color, color =>
			{
				var current = levelInfo.Value;
				if (current?.SongInfo == null) return;
				if (!current.SongInfo.Difficulties.TryGetValue(id, out var target)) return;

				target.Color = color;
				if (customRows.TryGetValue(id, out var row)) row.colorIndicator.color = color;
				levelSaver.SaveSettings();
			});
		}

		private void OnDeleteRequested(int id)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo == null) return;

			var name = info.SongInfo.Difficulties.TryGetValue(id, out var difficultyInfo)
				? difficultyInfo.Name
				: id.ToString();
			messageBox.ShowConfirmAndCancel("EditorEntry_DeleteCustomDifficultyConfirm", choice =>
			{
				if (choice == 0) DeleteCustomDifficulty(id);
			}, true, name);
		}

		private void DeleteCustomDifficulty(int id)
		{
			var info = levelInfo.Value;
			if (info?.SongInfo == null) return;

			var wasCurrent = info.Difficulty == id;
			info.SongInfo.Difficulties.Remove(id);

			var projectSetting = ISetting<T3ProjSetting>.Load(info.LevelPath);
			var chartName = projectSetting.GetChartFileName(id);
			var folder = Path.GetDirectoryName(info.LevelPath)!;
			DeleteIfExists(Path.Combine(folder, $"{chartName}.json"));
			DeleteIfExists(Path.Combine(folder, $"{chartName}.editing.json"));

			levelSaver.SaveSettings();
			SyncCustomToggles(info);

			// Deleting the difficulty being edited must move the editor onto another one.
			if (wasCurrent)
				levelLoader.LoadLevel(folder, info.SongInfo.Difficulties.Keys.DefaultIfEmpty(3).Max());

			void DeleteIfExists(string path)
			{
				if (File.Exists(path)) File.Delete(path);
			}
		}

		private void SyncCustomToggles(LevelInfo info)
		{
			var ids = DifficultyCatalog.GetCustomIds(info.SongInfo);

			foreach (var id in new List<int>(customRows.Keys))
			{
				if (!ids.Contains(id)) DestroyRow(id);
			}

			foreach (var id in ids)
			{
				if (customRows.ContainsKey(id)) RefreshRow(id, info);
				else CreateRow(id, info);
			}
		}

		private void CreateRow(int id, LevelInfo info)
		{
			var rowObject = Instantiate(customTogglePrefab, toggleParent);
			rowObject.name = $"CustomDifficultyToggle_{id}";
			var ui = rowObject.GetComponent<CustomDifficultyToggleUI>();

			var row = new CustomDifficultyRow
			{
				gameObject = rowObject,
				nameInputField = ui.NameInputField,
				colorIndicator = ui.ColorIndicator
			};

			row.registrars.Add(new InputFieldRegistrar(
				ui.NameInputField, InputFieldRegistrar.RegisterTarget.OnEndEdit, value => OnNameChanged(id, value)));
			row.registrars.Add(new ButtonRegistrar(ui.ColorButton, () => OnPickColor(id)));
			row.registrars.Add(new ButtonRegistrar(ui.DeleteButton, () => OnDeleteRequested(id)));
			foreach (var registrar in row.registrars) registrar.Register();

			customRows[id] = row;
			toggleGroup.AddToggle(id, new EditorDifficultyToggleData
			{
				toggle = ui.Toggle,
				difficultyText = ui.NameInputField.textComponent,
				levelInputField = ui.LevelInputField
			});
			RefreshRow(id, info);
		}

		private void RefreshRow(int id, LevelInfo info)
		{
			if (!customRows.TryGetValue(id, out var row)) return;
			info.SongInfo.Difficulties.TryGetValue(id, out var difficultyInfo);
			row.nameInputField.SetTextWithoutNotify(difficultyInfo?.Name ?? string.Empty);
			row.colorIndicator.color = difficultyInfo?.Color ?? Color.white;
		}

		private void DestroyRow(int id)
		{
			if (!customRows.TryGetValue(id, out var row)) return;

			foreach (var registrar in row.registrars) registrar.Unregister();
			toggleGroup.RemoveToggle(id);
			Destroy(row.gameObject);
			customRows.Remove(id);
		}

		private class CustomDifficultyRow
		{
			public GameObject gameObject = default!;
			public TMP_InputField nameInputField = default!;
			public Image colorIndicator = default!;
			public readonly List<IEventRegistrar> registrars = new();
		}
	}
}