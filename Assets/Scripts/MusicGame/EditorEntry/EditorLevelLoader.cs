#nullable enable

using System;
using System.IO;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.LevelSelect;
using MusicGame.Models.JudgeLine;
using MusicGame.Utility.JsonV1ToV2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Plugins;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.EditorEntry
{
	public class EditorLevelLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
#if UNITY_EDITOR
		[SerializeField] private bool mockStart = true;
#endif
		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			ListDataset<LevelComponent<EditorPreference>> dataset)
		{
			this.levelInfo = levelInfo;
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private ListDataset<LevelComponent<EditorPreference>> dataset = default!;

		// Defined Functions
		public void LoadLevel(string projectFolderPath, int difficulty = 0)
		{
			if (!Directory.Exists(projectFolderPath)) return;
#if !UNITY_EDITOR
			try
			{
#endif
			// 1. Find .t3proj, or create it
			DirectoryInfo directoryInfo = new DirectoryInfo(projectFolderPath);
			string projectFileName = ISetting<T3ProjSetting>.GetFileName();
			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				if (fileInfo.Extension == ".t3proj")
				{
					projectFileName = fileInfo.Name;
					break;
				}
			}

			string projectSettingPath = Path.Combine(projectFolderPath, projectFileName);
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(projectSettingPath);
			// 2. Load music
			var musicPath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.MusicFileName);
			var music = AudioLoader.LoadAudioFile(musicPath);
			// 3. Load cover
			var coverPath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.CoverFileName);
			var cover = TextureLoader.LoadTexture2D(coverPath);
			// 5. Load song info
			var songInfoPath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.SongInfoFileName);
			var songInfo = ISetting<SongInfo>.Load(songInfoPath);
			// 6. Load preference
			var preferencePath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.PreferenceFileName);
			var preference = ISetting<EditorPreference>.Load(preferencePath);
			// 4. Load chart
			difficulty = difficulty is >= 1 and <= 5 ? difficulty : preference.Difficulty;
			var chartName = projectSetting.GetChartFileName(difficulty);
			var editingChartPath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, chartName + ".editing.json");
			if (!File.Exists(editingChartPath))
			{
				var chartPath = FileHelper.GetAbsolutePathFromRelative(projectSettingPath, chartName + ".json");
				if (!File.Exists(chartPath))
				{
					ChartInfo chartInfo = new();
					chartInfo.AddComponentGeneric(new StaticJudgeLine());
					File.WriteAllText(editingChartPath, JsonConvert.SerializeObject(chartInfo.GetSerializationToken()));
				}
				else
				{
					editingChartPath = chartPath;
				}
			}

			JObject? jObject;
			try
			{
				jObject = JObject.Parse(File.ReadAllText(editingChartPath));
			}
			catch (JsonReaderException)
			{
				T3Logger.Log("Notice", "App_InvalidChart", T3LogType.Error);
				jObject = new();
			}

			// Temp chart version identifier
			ChartInfo? chart;
			if (jObject["version"] is { } token && token.Value<int>() == 2) chart = ChartInfo.Deserialize(jObject);
			else chart = V1ToV2Converter.DeserializeFromV1(jObject);

			var info = new LevelInfo
			{
				LevelPath = projectSettingPath,
				Chart = chart,
				Music = music,
				Cover = cover,
				SongInfo = songInfo,
				Preference = preference,
				Difficulty = difficulty
			};

			levelInfo.Value = info;
			levelInfo.AddUpNotify();
			T3Logger.Log("Notice", "App_LoadComplete", T3LogType.Success);

			bool exist = false;
			foreach (var data in dataset)
			{
				if (data.Model.LevelPath == info.LevelPath)
				{
					exist = true;
					dataset.MoveToTop(data);
					break;
				}
			}

			if (!exist)
			{
				RawLevelInfo<EditorPreference>.FromLevelInfo(info).ContinueWith(rawInfo =>
				{
					dataset.Add(new(rawInfo));
					dataset.MoveToTop(dataset.Count - 1);
				});
			}

#if !UNITY_EDITOR
			}
			catch
			{
				T3Logger.Log("Notice", "App_LoadError", T3LogType.Error);
			}
#endif
		}

		// System Functions
		void Start()
		{
			var args = Environment.GetCommandLineArgs();
			if (args.Length == 2)
			{
				var targetPath = args[1];
				if (Directory.Exists(targetPath))
				{
					LoadLevel(targetPath);
				}
				else if (File.Exists(targetPath) && targetPath.EndsWith(".t3proj"))
				{
					LoadLevel(Path.GetDirectoryName(targetPath)!);
				}
			}

#if UNITY_EDITOR
			if (mockStart) LoadLevel(@"G:\TAKANACharts\Test");
#endif
		}
	}
}