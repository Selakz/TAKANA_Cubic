#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using Newtonsoft.Json;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level
{
	public class EditorLevelSaver : T3MonoBehaviour, ISelfInstaller
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("EditorBasic", "Save", () =>
			{
				if (levelInfo.Value is null) return;
				try
				{
					SaveSettings();
					SaveEditorChart();
				}
				catch (Exception ex)
				{
					Debug.Log(ex);
					T3Logger.Log("Notice", "App_SaveError", T3LogType.Error);
					return;
				}

				T3Logger.Log("Notice", "App_SaveComplete", T3LogType.Success);
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo) => this.levelInfo = levelInfo;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this).AsSelf();

		// Defined Functions
		// TODO: threading
		public void SavePlayableChart(string? filePath = null)
		{
			if (levelInfo.Value is not { } info) return;
			ChartInfo chart = (IChartSerializable.Clone(info.Chart) as ChartInfo)!;
			chart.EditorConfig.Clear();
			List<ChartComponent> toRemove = new();
			foreach (var component in chart)
			{
				if (component.Model.IsEditorOnly())
				{
					toRemove.Add(component);
					continue;
				}

				component.Model.EditorConfig.Clear();
			}

			foreach (var component in toRemove) chart.RemoveComponent(component);

			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(info.LevelPath);
			var chartName = projectSetting.GetChartFileName(info.Difficulty);
			var token = chart.GetSerializationToken();
			var chartPath =
				filePath ?? FileHelper.GetAbsolutePathFromRelative(info.LevelPath, chartName + ".json");
			File.WriteAllText(chartPath, JsonConvert.SerializeObject(token,
				ISingleton<EditorSetting>.Instance.SaveIndented ? Formatting.Indented : Formatting.None));
		}

		public void SaveEditorChart(string? filePath = null)
		{
			if (levelInfo.Value is not { } info) return;
			ChartInfo chart = (IChartSerializable.Clone(info.Chart) as ChartInfo)!;
			List<ChartComponent> toRemove = chart.Where(component => component.Model.IsEditorOnly()).ToList();
			foreach (var component in toRemove) chart.RemoveComponent(component);

			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(info.LevelPath);
			var chartName = projectSetting.GetChartFileName(info.Difficulty);
			var token = chart.GetSerializationToken();
			var chartPath =
				filePath ?? FileHelper.GetAbsolutePathFromRelative(info.LevelPath, chartName + ".editing.json");
			File.WriteAllText(chartPath, JsonConvert.SerializeObject(token,
				ISingleton<EditorSetting>.Instance.SaveIndented ? Formatting.Indented : Formatting.None));
		}

		public void SaveSettings()
		{
			if (levelInfo.Value is not { } info) return;
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(info.LevelPath);
			// 1. Save song info
			var songInfoPath =
				FileHelper.GetAbsolutePathFromRelative(info.LevelPath, projectSetting.SongInfoFileName);
			ISetting<SongInfo>.Save(info.SongInfo, songInfoPath);
			// 2. Save preference
			var preferencePath =
				FileHelper.GetAbsolutePathFromRelative(info.LevelPath, projectSetting.PreferenceFileName);
			EditorPreference preference = info.Preference as EditorPreference ?? new();
			preference.BpmList = info.Chart.GetsBpmList().ToDictionary();
			ISetting<EditorPreference>.Save(preference, preferencePath);
		}
	}
}