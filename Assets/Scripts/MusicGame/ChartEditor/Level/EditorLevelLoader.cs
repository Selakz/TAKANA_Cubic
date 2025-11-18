#nullable enable

using System;
using System.IO;
using MusicGame.ChartEditor.Message;
using MusicGame.Components.Chart;
using MusicGame.Components.JudgeLines;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Plugins;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Level
{
	public class EditorLevelLoader : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<LevelInfo?> levelInfoContainer = default!;

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
					chartInfo.AddComponent(new JudgeLine());
					File.WriteAllText(editingChartPath, JsonConvert.SerializeObject(chartInfo.GetSerializationToken()));
				}
				else
				{
					editingChartPath = chartPath;
				}
			}

			var json = File.ReadAllText(editingChartPath);
			var chart = ChartInfo.Deserialize(JObject.Parse(json));
			var levelInfo = new LevelInfo
			{
				LevelPath = projectSettingPath,
				Chart = chart,
				Music = music,
				Cover = cover,
				SongInfo = songInfo,
				Preference = preference,
				Difficulty = difficulty
			};

			levelInfoContainer.Property.Value = levelInfo;
			levelInfoContainer.Property.AddUpNotify();
			// TODO: Delete this event
			EventManager.Instance.Invoke("Level_OnLoad", levelInfo);
			HeaderMessage.Show("加载完成！", HeaderMessage.MessageType.Success);
#if !UNITY_EDITOR
			}
			catch
			{
				HeaderMessage.Show("读取工程时发生错误，请检查路径和工程格式", HeaderMessage.MessageType.Error);
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
		}
	}
}