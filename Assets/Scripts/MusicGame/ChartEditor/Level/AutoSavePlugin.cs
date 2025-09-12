#nullable enable

using System;
using System.IO;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.Timer;
using UnityEngine;

namespace MusicGame.ChartEditor.Level
{
	public class AutoSavePlugin : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private EditorLevelSaver editorLevelSaver = default!;

		public T3Time AutoSaveInterval
		{
			get => Mathf.Max(60000, ISingletonSetting<EditorSetting>.Instance.AutoSaveInterval);
			set
			{
				ISingletonSetting<EditorSetting>.Instance.AutoSaveInterval = value;
				timer.TimeDelta = value;
				timer.Reset();
			}
		}

		// Private
		private TriggerTimer timer = default!;

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			timer.Start();
		}

		private void AutoSave()
		{
			Debug.Log("AutoSave");
			var levelInfo = LevelManager.Instance.LevelInfo;
			var folderPath = FileHelper.GetAbsolutePathFromRelative(levelInfo.LevelPath, "AutoSave");
			Directory.CreateDirectory(folderPath);
			T3ProjSetting projectSetting = ISetting<T3ProjSetting>.Load(levelInfo.LevelPath);
			var fileName =
				$"{DateTime.Now:yyyy-MM-dd_HH-mm}_{projectSetting.GetChartFileName(levelInfo.Difficulty)}.editing.json";
			editorLevelSaver.SaveEditorChart(Path.Combine(folderPath, fileName));
			editorLevelSaver.SaveSettings();
		}

		// System Functions
		void Awake()
		{
			timer = new TriggerTimer(AutoSaveInterval);
			timer.ShouldRepeat = true;
			timer.OnTrigger += AutoSave;
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDestroy()
		{
			timer.OnTrigger -= AutoSave;
			timer.Dispose();
		}
	}
}