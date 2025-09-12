using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class OffsetInputField : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField offsetInputField;

		// Private
		private T3Time Offset
		{
			set => offsetInputField.SetTextWithoutNotify(value.ToString());
		}

		// Static

		// Defined Function

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			offsetInputField.interactable = true;
			Offset = levelInfo.Chart.Properties.Get("offset", 0);
		}

		private void OnOffsetInputFieldEndEdit(string content)
		{
			if (int.TryParse(content, out int offset) && offset >= 0)
			{
				LevelManager.Instance.Music.Offset = offset;
				IEditingChartManager.Instance.Chart.Properties["offset"] = offset;
			}
			else
			{
				Offset = LevelManager.Instance.Music.Offset;
			}
		}

		// System Function
		void Awake()
		{
			offsetInputField.onEndEdit.AddListener(OnOffsetInputFieldEndEdit);
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