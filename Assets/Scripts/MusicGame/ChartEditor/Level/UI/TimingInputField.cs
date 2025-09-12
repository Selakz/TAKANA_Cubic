using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.Level.UI
{
	public class TimingInputField : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_InputField inputField;

		// Private

		// Static

		// Defined Function

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			inputField.interactable = true;
		}

		private void OnInputFieldEndEdit(string useless)
		{
			T3Time targetTime = int.Parse(inputField.text);
			EventManager.Instance.Invoke("Level_OnReset", targetTime);
		}

		// System Function
		void OnEnable()
		{
			inputField.onEndEdit.AddListener(OnInputFieldEndEdit);
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void OnDisable()
		{
			inputField.onEndEdit.RemoveListener(OnInputFieldEndEdit);
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}

		void Update()
		{
			if (!inputField.isFocused)
			{
				inputField.SetTextWithoutNotify(LevelManager.Instance.Music.ChartTime.ToString());
			}
		}
	}
}