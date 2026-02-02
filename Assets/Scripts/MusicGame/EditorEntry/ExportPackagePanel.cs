#nullable enable

using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.JudgeLine;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MusicGame.EditorEntry
{
	public class ExportPackagePanel : HierarchySystem<ExportPackagePanel>
	{
		// Serializable and Public
		[SerializeField] private UrlInputField exportPathInputField = default!;
		[SerializeField] private I18NTextBlock exportHintText = default!;
		[SerializeField] private Button exportButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, UpdatePanel),
			new PropertyRegistrar<string>(exportPathInputField.Path, UpdatePanel),
			new ButtonRegistrar(exportButton, ExportProjectAsync)
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelInfo = levelInfo;
		}

		// Defined Functions
		private void UpdatePanel()
		{
			if (levelInfo.Value is null)
			{
				exportHintText.SetText("EditorEntry_InvalidCurrentProject");
				exportButton.interactable = false;
				return;
			}

			bool valid = exportPathInputField.IsPathValid;
			var path = exportPathInputField.Path.Value;
			exportHintText.SetText(
				valid ? "EditorEntry_TargetT3pkg" : "EditorEntry_InvalidDirectory",
				valid ? Path.Combine(path, GetPackageName()) : path);
			exportButton.interactable = valid;
		}

		private string GetPackageName()
		{
			if (levelInfo.Value is not { } info) return string.Empty;
			var projName = Path.GetFileNameWithoutExtension(info.LevelPath);
			var id = info.SongInfo.Id;
			var fileName = string.IsNullOrWhiteSpace(projName)
				? string.IsNullOrWhiteSpace(id) || !FileHelper.IsValidPath(id) ? "package" : id
				: projName;
			return $"{fileName}.t3pkg";
		}

		private async void ExportProjectAsync()
		{
			if (!exportPathInputField.IsPathValid || levelInfo.Value is not { } info) return;

			string zipPath = Path.Combine(exportPathInputField.InputField.text, GetPackageName());
			await using FileStream zipFile = File.Create(zipPath);
			using ZipArchive archive = new(zipFile, ZipArchiveMode.Create);
			var projectSetting = await ISetting<T3ProjSetting>.LoadAsync(info.LevelPath);

			// 0. t3proj
			ZipArchiveEntry projZipEntry = archive.CreateEntry(Path.GetFileName(info.LevelPath));
			await using (Stream entryStream = projZipEntry.Open())
			{
				byte[] fileData = System.Text.Encoding.UTF8.GetBytes(ISetting<T3ProjSetting>.ToString(projectSetting));
				await entryStream.WriteAsync(fileData, 0, fileData.Length);
			}

			// 1. chart.json
			foreach (var difficulty in info.SongInfo.Difficulties.Keys.ToArray())
			{
				var editingChart = await ChartLoader.LoadFromEditorProject(
					info.LevelPath, projectSetting, difficulty).AsUniTask();
				if (editingChart is null || editingChart.Count == 0 ||
				    (editingChart.Count == 1 && editingChart.Components.First().Model is StaticJudgeLine))
				{
					info.SongInfo.Difficulties.Remove(difficulty);
				}
				else
				{
					var chart = EditorLevelSaver.GetPlayableChart(editingChart);
					var json = chart.GetSerializationToken().ToString();
					var chartEntryName = $"{projectSetting.GetChartFileName(difficulty)}.json";
					ZipArchiveEntry chartZipEntry = archive.CreateEntry(chartEntryName);
					await using Stream entryStream = chartZipEntry.Open();
					byte[] fileData = System.Text.Encoding.UTF8.GetBytes(json);
					await entryStream.WriteAsync(fileData, 0, fileData.Length);
				}
			}

			// 2. songinfo.yaml
			ZipArchiveEntry infoZipEntry = archive.CreateEntry(projectSetting.SongInfoFileName);
			await using (Stream entryStream = infoZipEntry.Open())
			{
				byte[] fileData = System.Text.Encoding.UTF8.GetBytes(ISetting<SongInfo>.ToString(info.SongInfo));
				await entryStream.WriteAsync(fileData, 0, fileData.Length);
			}

			// 3. music
			var musicPath = FileHelper.GetAbsolutePathFromRelative(info.LevelPath, projectSetting.MusicFileName);
			if (File.Exists(musicPath))
			{
				ZipArchiveEntry songZipEntry = archive.CreateEntry(projectSetting.MusicFileName);
				await using var fileStream = new FileStream(musicPath, FileMode.Open, FileAccess.Read);
				await using var entryStream = songZipEntry.Open();
				await fileStream.CopyToAsync(entryStream);
			}

			// 4. cover
			var coverPath = FileHelper.GetAbsolutePathFromRelative(info.LevelPath, projectSetting.CoverFileName);
			if (File.Exists(coverPath))
			{
				ZipArchiveEntry coverZipEntry = archive.CreateEntry(projectSetting.CoverFileName);
				await using var fileStream = new FileStream(coverPath, FileMode.Open, FileAccess.Read);
				await using var entryStream = coverZipEntry.Open();
				await fileStream.CopyToAsync(entryStream);
			}

			// 5. preference.yaml
			var prefPath = FileHelper.GetAbsolutePathFromRelative(info.LevelPath, projectSetting.PreferenceFileName);
			if (File.Exists(prefPath))
			{
				ZipArchiveEntry prefZipEntry = archive.CreateEntry(projectSetting.PreferenceFileName);
				await using var fileStream = new FileStream(prefPath, FileMode.Open, FileAccess.Read);
				await using var entryStream = prefZipEntry.Open();
				await fileStream.CopyToAsync(entryStream);
			}

			ISingletonSetting<EditorSetting>.Instance.LastExportDirectory.Value = exportPathInputField.Path;
			ISingletonSetting<EditorSetting>.SaveInstance();
			Process.Start(new ProcessStartInfo
			{
				FileName = exportPathInputField.Path,
				UseShellExecute = true,
				Verb = "open"
			});
		}

		// System Functions
		void Start()
		{
			exportPathInputField.InputField.text = ISingletonSetting<EditorSetting>.Instance.LastExportDirectory;
		}
	}
}