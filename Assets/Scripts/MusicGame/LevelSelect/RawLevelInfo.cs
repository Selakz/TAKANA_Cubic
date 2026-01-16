#nullable enable

using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Utility.JsonV1ToV2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Plugins;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.LevelSelect
{
	public class RawLevelInfo
	{
		public string LevelPath { get; set; } = string.Empty;

		public T3ProjSetting? ProjectSetting { get; set; }

		public NotifiableProperty<Texture2D?> Cover { get; } = new(null);

		public NotifiableProperty<SongInfo?> SongInfo { get; } = new(null);

		public NotifiableProperty<GameplayPreference?> Preference { get; } = new(null);

		private LevelInfo? levelInfo;

		public static async UniTask<RawLevelInfo?> FromFolder(string folderPath)
		{
			// 1. Find .t3proj, or create it
			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			if (!directoryInfo.Exists) return null;

			string projectFileName = ISetting<T3ProjSetting>.GetFileName();
			foreach (FileInfo fileInfo in directoryInfo.GetFiles())
			{
				if (fileInfo.Extension == ".t3proj")
				{
					projectFileName = fileInfo.Name;
					break;
				}
			}

			string projectSettingPath = Path.Combine(folderPath, projectFileName);
			if (!File.Exists(projectSettingPath)) return null;
			T3ProjSetting projectSetting = await ISetting<T3ProjSetting>.LoadAsync(projectSettingPath);

			var rawLevelInfo = new RawLevelInfo { LevelPath = projectSettingPath, ProjectSetting = projectSetting };
			// 2. Load song info fire-and-forget
			var songInfoPath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.SongInfoFileName);
			ISetting<SongInfo>.LoadAsync(songInfoPath).AsUniTask()
				.ContinueWith(songInfo => rawLevelInfo.SongInfo.Value = songInfo)
				.Forget();

			// 3. Load preference fire-and-forget
			var preferencePath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.PreferenceFileName);
			ISetting<GameplayPreference>.LoadAsync(preferencePath).AsUniTask()
				.ContinueWith(preference => rawLevelInfo.Preference.Value = preference)
				.Forget();

			// 4. Load cover fire-and-forget
			var coverPath =
				FileHelper.GetAbsolutePathFromRelative(projectSettingPath, projectSetting.CoverFileName);
			TextureLoader.LoadTexture2DAsync(coverPath)
				.ContinueWith(texture => rawLevelInfo.Cover.Value = texture)
				.Forget();

			return rawLevelInfo;
		}

		public static async UniTask<RawLevelInfo> FromLevelInfo(LevelInfo levelInfo)
		{
			var rawLevelInfo = new RawLevelInfo
			{
				LevelPath = levelInfo.LevelPath,
				ProjectSetting = await ISetting<T3ProjSetting>.LoadAsync(levelInfo.LevelPath),
				Cover = { Value = levelInfo.Cover },
				SongInfo = { Value = levelInfo.SongInfo },

				levelInfo = levelInfo
			};
			return rawLevelInfo;
		}

		public async UniTask<LevelInfo?> ToLevelInfo(int difficulty = 0)
		{
			if (levelInfo is not null && levelInfo.Difficulty == difficulty)
			{
				Debug.Log($"Difficulty equals: {difficulty}");
				return levelInfo;
			}

			if (ProjectSetting is not { } setting ||
			    SongInfo.Value is not { } songInfo ||
			    Preference.Value is not { } preference) return null;

			// 5. Load chart
			difficulty = difficulty is >= 1 and <= 5 ? difficulty : preference.Difficulty;
			var chartName = setting.GetChartFileName(difficulty);
			ChartInfo? chart = null;
			var chartFileName = $"{chartName}.json";
			var chartPath = FileHelper.GetAbsolutePathFromRelative(LevelPath, chartFileName);
			if (File.Exists(chartPath)) chart = await TempLoadChart(chartPath);
			if (chart is null) return null;

			if (levelInfo is not null)
			{
				levelInfo.Chart = chart;
				levelInfo.Difficulty = difficulty;
				return levelInfo;
			}

			// 6. Load music
			var musicPath =
				FileHelper.GetAbsolutePathFromRelative(LevelPath, setting.MusicFileName);
			var music = await AudioLoader.LoadAudioFileAsync(musicPath);
			if (music is null) return null;

			return levelInfo = new LevelInfo
			{
				LevelPath = LevelPath,
				Chart = chart,
				Music = music,
				Cover = Cover.Value,
				SongInfo = songInfo,
				Preference = preference,
				Difficulty = difficulty
			};

			async Task<ChartInfo?> TempLoadChart(string path)
			{
				JObject? jObject;
				try
				{
					jObject = JObject.Parse(await File.ReadAllTextAsync(path));
				}
				catch (JsonReaderException)
				{
					T3Logger.Log("Notice", "App_InvalidChart", T3LogType.Error);
					return null;
				}

				// Temp chart version identifier
				if (jObject["version"] is { } token && token.Value<int>() == 2)
					chart = ChartInfo.Deserialize(jObject);
				else chart = V1ToV2Converter.DeserializeFromV1(jObject);
				return chart;
			}
		}
	}

	public class LevelComponent : IComponent<RawLevelInfo>
	{
		public RawLevelInfo Model { get; }

		public event EventHandler? OnComponentUpdated;

		public LevelComponent(RawLevelInfo model)
		{
			Model = model;
			Model.SongInfo.PropertyChanged += (_, _) => UpdateNotify();
			Model.Cover.PropertyChanged += (_, _) => UpdateNotify();
			Model.Preference.PropertyChanged += (_, _) => UpdateNotify();
		}

		public void UpdateModel(Action<RawLevelInfo> action)
		{
			action.Invoke(Model);
			UpdateNotify();
		}

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);
	}
}