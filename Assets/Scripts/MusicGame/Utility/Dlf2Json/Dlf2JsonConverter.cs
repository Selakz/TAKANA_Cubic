using System;
using System.IO;
using MusicGame.ChartEditor.Message;
using Newtonsoft.Json;
using UnityEngine;

namespace MusicGame.Utility.Dlf2Json
{
	public class Dlf2JsonConverter : MonoBehaviour
	{
		// System Functions
		void Start()
		{
			var args = Environment.GetCommandLineArgs();
			if (args.Length == 2)
			{
				var targetPath = args[1];
				if (File.Exists(targetPath) && targetPath.EndsWith(".dlf"))
				{
					string content = File.ReadAllText(targetPath);
					try
					{
						var chartInfo = LegacyChartReader.Read(content);
						var token = chartInfo.GetSerializationToken();
						var outputPath = targetPath.Replace(".dlf", ".json");
						File.WriteAllText(outputPath, JsonConvert.SerializeObject(token, Formatting.Indented));
						HeaderMessage.Show("成功将dlf转换为新格式谱面，保存在相同文件夹中", HeaderMessage.MessageType.Success);
					}
					catch (Exception exception)
					{
						HeaderMessage.Show(exception.Message, HeaderMessage.MessageType.Error);
						Debug.Log(exception.StackTrace);
					}
				}
			}
		}
	}
}