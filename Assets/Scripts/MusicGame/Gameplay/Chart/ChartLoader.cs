#nullable enable

using System.IO;
using System.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Models.JudgeLine;
using MusicGame.Utility.JsonV1ToV2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Log;

namespace MusicGame.Gameplay.Chart
{
	public static class ChartLoader
	{
		public static async Task<ChartInfo?> LoadFromEditorProject(
			string levelPath, T3ProjSetting projectSetting, int difficulty)
		{
			var chartName = projectSetting.GetChartFileName(difficulty);
			var editingChartPath = FileHelper.GetAbsolutePathFromRelative(levelPath, chartName + ".editing.json");
			if (!File.Exists(editingChartPath))
			{
				var chartPath = FileHelper.GetAbsolutePathFromRelative(levelPath, chartName + ".json");
				if (!File.Exists(chartPath))
				{
					ChartInfo chartInfo = new();
					chartInfo.AddComponentGeneric(new StaticJudgeLine());
					await File.WriteAllTextAsync(
						editingChartPath, JsonConvert.SerializeObject(chartInfo.GetSerializationToken()));
				}
				else
				{
					editingChartPath = chartPath;
				}
			}

			return await LoadFromFile(editingChartPath);
		}

		public static async Task<ChartInfo?> LoadFromFile(string path)
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
			if (jObject["version"] is { } token && token.Value<int>() >= 2) return ChartInfo.Deserialize(jObject);
			else return V1ToV2Converter.DeserializeFromV1(jObject);
		}
	}
}