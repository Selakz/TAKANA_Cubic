#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.Message;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using TMPro;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class EaseSwitcher : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject headerCurveTextObject = default!;
		[SerializeField] private TMP_Text currentCurveText = default!;

		// Private
		private Dictionary<int, string> easeIdToName = default!;

		// Event Handlers
		private void LevelOnLoad(LevelInfo levelInfo)
		{
			var easeId = ISingletonSetting<TrackLineSetting>.Instance.DefaultEaseId;
			easeId = Mathf.Clamp(easeId, 0, 9);
			TrackMovementEditingManager.Instance.CurrentEaseId = easeId;
		}

		private void ChangeEase(int id)
		{
			TrackMovementEditingManager.Instance.CurrentEaseId = id;
			HeaderMessage.Show($"切换至{easeIdToName[id]}曲线族", HeaderMessage.MessageType.Info);
		}

		private void CheckEase()
		{
			currentCurveText.text = $"当前选中曲线族：{easeIdToName[TrackMovementEditingManager.Instance.CurrentEaseId]}";
			headerCurveTextObject.SetActive(true);
		}

		private void HideEase()
		{
			headerCurveTextObject.SetActive(false);
		}

		// System Functions
		void Awake()
		{
			easeIdToName = new()
			{
				{ 1, "Sine" },
				{ 2, "Quad" },
				{ 3, "Cubic" },
				{ 4, "Quart" },
				{ 5, "Quint" },
				{ 6, "Expo" },
				{ 7, "Circ" },
				{ 8, "Back" },
				{ 9, "Elastic" },
				{ 0, "Bounce" },
			};
		}

		void OnEnable()
		{
			EventManager.Instance.AddListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
			InputManager.Instance.Register("CurveSwitch", "SwitchToSine", _ => ChangeEase(1));
			InputManager.Instance.Register("CurveSwitch", "SwitchToQuad", _ => ChangeEase(2));
			InputManager.Instance.Register("CurveSwitch", "SwitchToCubic", _ => ChangeEase(3));
			InputManager.Instance.Register("CurveSwitch", "SwitchToQuart", _ => ChangeEase(4));
			InputManager.Instance.Register("CurveSwitch", "SwitchToQuint", _ => ChangeEase(5));
			InputManager.Instance.Register("CurveSwitch", "SwitchToExpo", _ => ChangeEase(6));
			InputManager.Instance.Register("CurveSwitch", "SwitchToCirc", _ => ChangeEase(7));
			InputManager.Instance.Register("CurveSwitch", "SwitchToBack", _ => ChangeEase(8));
			InputManager.Instance.Register("CurveSwitch", "SwitchToElastic", _ => ChangeEase(9));
			InputManager.Instance.Register("CurveSwitch", "SwitchToBounce", _ => ChangeEase(0));
			InputManager.Instance.RegisterStarted("CurveSwitch", "CheckCurve", _ => CheckEase());
			InputManager.Instance.RegisterCanceled("CurveSwitch", "CheckCurve", _ => HideEase());
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<LevelInfo>("Level_OnLoad", LevelOnLoad);
		}
	}
}