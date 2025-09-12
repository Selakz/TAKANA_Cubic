using MusicGame.Gameplay;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TimeIndicator : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TMP_Text timeText;
		[SerializeField] private GameObject indicator;

		// Private

		// Static

		// Defined Functions

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			indicator.SetActive(true);
		}

		// System Functions
		void Update()
		{
			var mousePosition = Input.mousePosition;
			if (!LevelManager.Instance.LevelCamera.ContainsScreenPoint(mousePosition))
			{
				indicator.transform.localPosition = new(0, ISingletonSetting<PlayfieldSetting>.Instance.UpperThreshold);
			}
			else if (indicator.activeSelf)
			{
				var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(mousePosition);
				var time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
				var y = (time - LevelManager.Instance.Music.ChartTime) * LevelManager.Instance.LevelSpeed.SpeedRate;
				indicator.transform.localPosition = new(0, y);
				timeText.text = time.ToString();
			}
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