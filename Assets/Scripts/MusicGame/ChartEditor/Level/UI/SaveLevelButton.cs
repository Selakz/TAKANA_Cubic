#nullable enable

using System;
using MusicGame.ChartEditor.Message;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SaveLevelButton : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private EditorLevelSaver levelSaver = default!;
		[SerializeField] private Button saveEditingLevelButton = default!;
		[SerializeField] private Button savePlayableLevelButton = default!;

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			saveEditingLevelButton.interactable = true;
			savePlayableLevelButton.interactable = true;
		}

		private void OnSaveEditingLevelButtonClick()
		{
			try
			{
				levelSaver.SaveSettings();
				levelSaver.SaveEditorChart();
			}
			catch (Exception ex)
			{
				Debug.Log(ex);
				HeaderMessage.Show("工程保存过程中出现异常", HeaderMessage.MessageType.Error);
				return;
			}

			HeaderMessage.Show("保存成功！", HeaderMessage.MessageType.Success);
		}

		private void OnSavePlayableLevelButtonClick()
		{
			try
			{
				levelSaver.SavePlayableChart();
			}
			catch
			{
				HeaderMessage.Show("导出过程中出现异常", HeaderMessage.MessageType.Error);
				return;
			}

			HeaderMessage.Show("成功导出无编辑时信息谱面", HeaderMessage.MessageType.Success);
		}

		// System Functions
		void Awake()
		{
			saveEditingLevelButton.onClick.AddListener(OnSaveEditingLevelButtonClick);
			savePlayableLevelButton.onClick.AddListener(OnSavePlayableLevelButtonClick);
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}