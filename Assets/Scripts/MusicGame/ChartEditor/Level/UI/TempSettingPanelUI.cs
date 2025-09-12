#nullable enable

using System.Collections.Generic;
using System.IO;
using MusicGame.ChartEditor.TrackLine;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Plugins;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class TempSettingPanelUI : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AutoSavePlugin autoSavePlugin = default!;
		[SerializeField] private TMP_InputField autoSaveIntervalInputField = default!;
		[SerializeField] private TMP_InputField scrollSensitivityInputField = default!;
		[SerializeField] private TMP_Dropdown defaultEaseDropdown = default!;
		[SerializeField] private Button openSettingsButton = default!;

		// Private
		private int[] easeIds = default!;

		// Event Handlers
		private void OnAutoSaveIntervalInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int autoSaveInterval) && autoSaveInterval > 0)
			{
				ISingletonSetting<EditorSetting>.Instance.AutoSaveInterval = autoSaveInterval * 60000;
				ISingletonSetting<EditorSetting>.SaveInstance();
				autoSavePlugin.AutoSaveInterval = autoSaveInterval * 60000;
				return;
			}

			autoSaveIntervalInputField.text =
				(ISingletonSetting<EditorSetting>.Instance.AutoSaveInterval / 60000).ToString();
		}

		private void OnScrollSensitivityInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int scrollSensitivity))
			{
				ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity = scrollSensitivity;
				ISingletonSetting<EditorSetting>.SaveInstance();
				return;
			}

			scrollSensitivityInputField.text = ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity.ToString();
		}

		private void OnDefaultEaseDropdownValueChanged(int choice)
		{
			ISingletonSetting<TrackLineSetting>.Instance.DefaultEaseId = easeIds[choice];
			ISingletonSetting<TrackLineSetting>.SaveInstance();
		}

		private void OnOpenSettingButtonClick()
		{
			FileBrowser.OpenExplorer(Path.Combine(Application.persistentDataPath, "Settings"));
		}

		// System Functions
		void Awake()
		{
			easeIds = defaultEaseDropdown.SetOptions(
				new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 },
				id => id switch
				{
					1 => "Sine",
					2 => "Quad",
					3 => "Cubic",
					4 => "Quart",
					5 => "Quint",
					6 => "Expo",
					7 => "Circ",
					8 => "Back",
					9 => "Elastic",
					0 => "Bounce",
					_ => string.Empty
				}
			);
			autoSaveIntervalInputField.onEndEdit.AddListener(OnAutoSaveIntervalInputFieldEndEdit);
			scrollSensitivityInputField.onEndEdit.AddListener(OnScrollSensitivityInputFieldEndEdit);
			defaultEaseDropdown.onValueChanged.AddListener(OnDefaultEaseDropdownValueChanged);
			openSettingsButton.onClick.AddListener(OnOpenSettingButtonClick);
		}

		void OnEnable()
		{
			autoSaveIntervalInputField.SetTextWithoutNotify(
				(ISingletonSetting<EditorSetting>.Instance.AutoSaveInterval / 60000).ToString());
			scrollSensitivityInputField.SetTextWithoutNotify(
				ISingletonSetting<EditorSetting>.Instance.ScrollSensitivity.ToString());
			for (int i = 0; i < easeIds.Length; ++i)
			{
				if (easeIds[i] == ISingletonSetting<TrackLineSetting>.Instance.DefaultEaseId)
				{
					defaultEaseDropdown.SetValueWithoutNotify(i);
					break;
				}
			}
		}
	}
}