#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MusicGame.ChartEditor.Level;
using MusicGame.Utility.Dakumi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using T3Framework.Preset.Event;
using T3Framework.Preset.UICollection;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace MusicGame.EditorEntry
{
	public enum FromChartType
	{
		Dakumi,
	}

	public class ChartConversionPanel : HierarchySystem<ChartConversionPanel>
	{
		// Serializable and Public
		[SerializeField] private TMP_Dropdown convertTypeDropdown = default!;
		[SerializeField] private UrlInputField chartPathInputField = default!;
		[SerializeField] private I18NTextBlock exportHintText = default!;
		[SerializeField] private Button convertButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<string>(chartPathInputField.Path, UpdatePanel),
			new ButtonRegistrar(convertButton, ConvertProjectAsync)
		};

		// Private
		private FromChartType[]? options;
		private readonly List<FromChartType> fromChartTypes = new() { FromChartType.Dakumi };

		// Defined Functions
		private string GetConvertedPath()
		{
			if (!chartPathInputField.IsPathValid) return string.Empty;
			var path = chartPathInputField.Path.Value;
			var fileName = Path.GetFileNameWithoutExtension(path);
			return FileHelper.GetAbsolutePathFromRelative(path, $"{fileName}.editing.json");
		}

		private void UpdatePanel()
		{
			bool valid = chartPathInputField.IsPathValid;
			var path = chartPathInputField.Path.Value;
			exportHintText.SetText(
				valid ? "EditorEntry_TargetTakana" : "EditorEntry_InvalidFile",
				valid ? GetConvertedPath() : path);
			convertButton.interactable = valid;
		}

		private async void ConvertProjectAsync()
		{
			if (!chartPathInputField.IsPathValid) return;
			var chartType = options?[convertTypeDropdown.value] ?? FromChartType.Dakumi;
			var path = chartPathInputField.Path.Value;
			var content = await File.ReadAllTextAsync(path);
			var convertedPath = GetConvertedPath();
			try
			{
				switch (chartType)
				{
					case FromChartType.Dakumi:
						var dakumiChart = DakumiChart.FromJObject(JObject.Parse(content));
						var chart = DakumiToTakanaConverter.DeserializeFromDakumi(dakumiChart);
						var token = chart.GetSerializationToken();
						await File.WriteAllTextAsync(convertedPath, JsonConvert.SerializeObject(token,
							ISingleton<EditorSetting>.Instance.SaveIndented ? Formatting.Indented : Formatting.None));
						break;
				}

				Process.Start(new ProcessStartInfo
				{
					FileName = Path.GetDirectoryName(convertedPath) ?? string.Empty,
					UseShellExecute = true,
					Verb = "open"
				});
			}
			catch (Exception e)
			{
				Debug.LogError($"Failed to convert from {path}, error: {e.Message}\n{e.StackTrace}");
			}
		}

		// System Functions
		void Start()
		{
			options = convertTypeDropdown.SetOptions(fromChartTypes, chartType => chartType.ToString());
		}
	}
}