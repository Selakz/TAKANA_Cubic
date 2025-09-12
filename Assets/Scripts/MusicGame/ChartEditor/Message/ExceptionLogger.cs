#nullable enable

using System;
using System.IO;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace MusicGame.ChartEditor.Message
{
	public class ExceptionLogger : MonoBehaviour
	{
		// Private
		private bool isQuitting = false;
		private string logFolderPath = Path.Combine(Application.streamingAssetsPath, "Logs");

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			logFolderPath = FileHelper.GetAbsolutePathFromRelative(levelInfo.LevelPath, "Logs");
		}

		private void LogToProject(string condition, string stackTrace, LogType logType)
		{
			if (isQuitting) return;
			if (logType is LogType.Error or LogType.Exception or LogType.Assert)
			{
				string logFilePath = Path.Combine(logFolderPath, $"{DateTime.Now:yyyy-MM-dd} - Exceptions.log");
				Directory.CreateDirectory(logFolderPath);
				File.AppendAllLines(logFilePath, new[]
				{
					$"[time]: {DateTime.Now}",
					$"[type]: {logType}",
					$"[exception message]: {condition}",
					$"[stack trace]: {stackTrace}",
					string.Empty
				});
			}
		}

		// System Functions
		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			Application.logMessageReceived += LogToProject;
			Application.wantsToQuit += () => isQuitting = true;
		}
	}
}