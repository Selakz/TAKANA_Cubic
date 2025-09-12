#nullable enable

using System;
using System.IO;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Message;
using MusicGame.Components.Chart;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Level
{
	public class EditorLevelSaver : MonoBehaviour
	{
		// Serializable and Public

		// Defined Functions
		// TODO: threading
		public void SavePlayableChart(string? filePath = null)
		{
			LevelInfo levelInfo = new();
			EventManager.Instance.Invoke("Level_BeforeSave", levelInfo);
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(levelInfo.LevelPath);
			var chartName = projectSetting.GetChartFileName(levelInfo.Difficulty);
			var token = GetPlayableChartInfo(levelInfo.Chart).GetSerializationToken();
			var chartPath =
				filePath ?? FileHelper.GetAbsolutePathFromRelative(levelInfo.LevelPath, chartName + ".json");
			File.WriteAllText(chartPath, JsonConvert.SerializeObject(token, Formatting.Indented));
		}

		public void SaveEditorChart(string? filePath = null)
		{
			LevelInfo levelInfo = new();
			EventManager.Instance.Invoke("Level_BeforeSave", levelInfo);
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(levelInfo.LevelPath);
			var chartName = projectSetting.GetChartFileName(levelInfo.Difficulty);
			var editingChartPath =
				filePath ?? FileHelper.GetAbsolutePathFromRelative(levelInfo.LevelPath, chartName + ".editing.json");
			var token = levelInfo.Chart.GetSerializationToken();
			File.WriteAllText(editingChartPath, JsonConvert.SerializeObject(token, Formatting.Indented));
		}

		public void SaveSettings()
		{
			LevelInfo levelInfo = new();
			EventManager.Instance.Invoke("Level_BeforeSave", levelInfo);
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(levelInfo.LevelPath);
			// 1. Save song info
			var songInfoPath =
				FileHelper.GetAbsolutePathFromRelative(levelInfo.LevelPath, projectSetting.SongInfoFileName);
			ISetting<SongInfo>.Save(levelInfo.SongInfo, songInfoPath);
			// 2. Save preference
			var preferencePath =
				FileHelper.GetAbsolutePathFromRelative(levelInfo.LevelPath, projectSetting.PreferenceFileName);
			EditorPreference preference = levelInfo.Preference as EditorPreference ?? new();
			ISetting<EditorPreference>.Save(preference, preferencePath);
		}

		private static ChartInfo GetPlayableChartInfo(ChartInfo editingChart)
		{
			ChartInfo playableChart = new()
			{
				Properties = new JObject(editingChart.Properties)
			};
			playableChart.Properties.Remove("editorconfig");
			foreach (var component in editingChart)
			{
				if (component is EditingComponent editingComponent)
				{
					playableChart.AddComponent(editingComponent.Component);
				}
				else
				{
					playableChart.AddComponent(component);
				}
			}

			return playableChart;
		}

		// System Functions
		void OnEnable()
		{
			InputManager.Instance.Register("EditorBasic", "Save", _ =>
			{
				// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
				if (LevelManager.Instance.LevelInfo is null) return;
				try
				{
					SaveSettings();
					SaveEditorChart();
				}
				catch (Exception ex)
				{
					Debug.Log(ex);
					HeaderMessage.Show("工程保存过程中出现异常", HeaderMessage.MessageType.Error);
					return;
				}

				HeaderMessage.Show("保存成功！", HeaderMessage.MessageType.Success);
			});
		}
	}
}