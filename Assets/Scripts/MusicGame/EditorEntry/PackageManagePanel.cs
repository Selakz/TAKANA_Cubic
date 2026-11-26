#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect;
using MusicGame.Models;
using MusicGame.Models.JudgeLine;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Debug = UnityEngine.Debug;

namespace MusicGame.EditorEntry
{
	public class PackageManagePanel : HierarchySystem<PackageManagePanel>
	{
		// Serializable and Public
		[SerializeField] private UrlInputField exportPathInputField = default!;
		[SerializeField] private I18NTextBlock exportHintText = default!;
		[SerializeField] private Button exportButton = default!;

		[SerializeField] private UrlInputField importPackageInputField = default!;
		[SerializeField] private I18NTextBlock importPackageHintText = default!;
		[SerializeField] private UrlInputField extractDirectoryInputField = default!;
		[SerializeField] private I18NTextBlock extractDirectoryHintText = default!;
		[SerializeField] private Toggle openAfterImportToggle = default!;
		[SerializeField] private Button importButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, UpdateExportPanel),
			new PropertyRegistrar<string>(exportPathInputField.Path, UpdateExportPanel),
			new ButtonRegistrar(exportButton, ExportProjectAsync),
			new PropertyRegistrar<string>(importPackageInputField.Path, UpdateImportPanel),
			new PropertyRegistrar<string>(extractDirectoryInputField.Path, UpdateImportPanel),
			new ButtonRegistrar(importButton, ImportProjectAsync)
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private ListDataset<LevelComponent<EditorPreference>> dataset = default!;
		private EditorLevelLoader levelLoader = default!;

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			ListDataset<LevelComponent<EditorPreference>> dataset,
			EditorLevelLoader levelLoader)
		{
			this.levelInfo = levelInfo;
			this.dataset = dataset;
			this.levelLoader = levelLoader;
		}

		// Defined Functions
		private void UpdateExportPanel()
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

		private void UpdateImportPanel()
		{
			bool isPackageValid = IsPackagePathValid();
			var packagePath = importPackageInputField.Path.Value;
			importPackageHintText.SetText(
				isPackageValid ? "EditorEntry_ValidT3pkg" : "EditorEntry_InvalidT3pkg",
				packagePath);

			bool isDirectoryValid = extractDirectoryInputField.IsPathValid;
			var extractDir = extractDirectoryInputField.Path.Value;
			if (!isDirectoryValid)
			{
				extractDirectoryHintText.SetText("EditorEntry_InvalidDirectory", extractDir);
			}
			else if (!isPackageValid)
			{
				extractDirectoryHintText.SetText("EditorEntry_SelectValidT3pkgFirst");
			}
			else
			{
				extractDirectoryHintText.SetText("EditorEntry_TargetExtractDirectory", GetExtractDirectory());
			}

			importButton.interactable = isPackageValid && isDirectoryValid;
		}

		private bool IsPackagePathValid()
		{
			return importPackageInputField.IsPathValid &&
			       Path.GetExtension(importPackageInputField.Path.Value)
				       .Equals(".t3pkg", StringComparison.OrdinalIgnoreCase);
		}

		private string GetExtractDirectory()
		{
			var packageName = Path.GetFileNameWithoutExtension(importPackageInputField.Path.Value);
			return Path.Combine(extractDirectoryInputField.Path.Value, packageName);
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
				byte[] fileData = Encoding.UTF8.GetBytes(ISetting<T3ProjSetting>.ToString(projectSetting));
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
					ChartInfo chart = IChartSerializable.Clone(editingChart);
					ChartModePreprocessor.Process(chart, false);
					var json = chart.GetSerializationToken().ToString();
					var chartEntryName = $"{projectSetting.GetChartFileName(difficulty)}.json";
					ZipArchiveEntry chartZipEntry = archive.CreateEntry(chartEntryName);
					await using Stream entryStream = chartZipEntry.Open();
					byte[] fileData = Encoding.UTF8.GetBytes(json);
					await entryStream.WriteAsync(fileData, 0, fileData.Length);
				}
			}

			// 2. songinfo.yaml
			ZipArchiveEntry infoZipEntry = archive.CreateEntry(projectSetting.SongInfoFileName);
			await using (Stream entryStream = infoZipEntry.Open())
			{
				byte[] fileData = Encoding.UTF8.GetBytes(ISetting<SongInfo>.ToString(info.SongInfo));
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

		private async void ImportProjectAsync()
		{
			if (!IsPackagePathValid() || !extractDirectoryInputField.IsPathValid) return;

			importButton.interactable = false;
			var packagePath = importPackageInputField.Path.Value;
			var targetDirectory = GetExtractDirectory();

			try
			{
				await UniTask.RunOnThreadPool(() =>
				{
					Directory.CreateDirectory(targetDirectory);
					ZipFile.ExtractToDirectory(packagePath, targetDirectory, true);
				});

				ISingletonSetting<EditorSetting>.Instance.LastProjectDirectory.Value =
					extractDirectoryInputField.Path.Value;
				await AddOrMoveToTopAsync(targetDirectory);
				T3Logger.Log("Notice", "EditorEntry_ImportSuccess", T3LogType.Success);

				if (openAfterImportToggle.isOn)
				{
					levelLoader.LoadLevel(targetDirectory);
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to extract t3pkg {packagePath}: {e.Message}\n{e.StackTrace}");
				T3Logger.Log("Notice", "EditorEntry_ImportFailed", T3LogType.Error);
			}
			finally
			{
				UpdateImportPanel();
			}

			return;

			async UniTask AddOrMoveToTopAsync(string projectDirectory)
			{
				var normalizedTarget = Path.GetFullPath(projectDirectory)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				foreach (var data in dataset)
				{
					var dir = Path.GetDirectoryName(data.Model.LevelPath);
					if (dir is not null && Path.GetFullPath(dir)
						    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
						    .Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase))
					{
						dataset.MoveToTop(data);
						return;
					}
				}

				var rawInfo = await RawLevelInfo<EditorPreference>.FromFolder(projectDirectory);
				if (rawInfo is not null)
				{
					dataset.Add(new(rawInfo));
					dataset.MoveToTop(dataset.Count - 1);
					return;
				}

				var recentProjects = ISingletonSetting<EditorSetting>.Instance.RecentProjects.Value;
				recentProjects.RemoveAll(p => Path.GetFullPath(p)
					.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					.Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase));
				recentProjects.Insert(0, projectDirectory);
				int maxCount = ISingletonSetting<EditorSetting>.Instance.RecentProjectBufferCount;
				if (recentProjects.Count > maxCount)
				{
					recentProjects.RemoveRange(maxCount, recentProjects.Count - maxCount);
				}

				ISingletonSetting<EditorSetting>.SaveInstance();
			}
		}

		// System Functions
		void Start()
		{
			exportPathInputField.InputField.text = ISingletonSetting<EditorSetting>.Instance.LastExportDirectory;
			extractDirectoryInputField.InputField.text = ISingletonSetting<EditorSetting>.Instance.LastProjectDirectory;
			UpdateExportPanel();
			UpdateImportPanel();
		}
	}
}