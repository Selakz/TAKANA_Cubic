using System;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Log;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Message
{
	public class HeaderMessage : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Image panelImage;
		[SerializeField] private I18NTextBlock messageText;
		[SerializeField] private Animator animator;

		public enum MessageType
		{
			Info,
			Warn,
			Error,
			Success
		}

		// Private
		private static HeaderMessage Instance { get; set; }

		// Static

		// Defined Functions
		public static void Show(string message, MessageType type)
		{
			Instance.panelImage.color = type.GetColor();
			Instance.messageText.SetText(message);
			Instance.animator.Play(0);
			Debug.Log($"HeaderMessage Show: {message}");
		}

		// Event Handlers
		private void ShowException(string condition, string stackTrace, LogType logType)
		{
			if (logType is LogType.Error or LogType.Exception or LogType.Assert)
				Show(I18NSystem.GetText("App_Exception"), MessageType.Error);
		}

		private static void OnLogNotice(string message, Enum type)
		{
			var split = message.Split('|');
			var text = I18NSystem.GetText(split[0], split[1..]);
			MessageType msgType = type is T3LogType t ? t.ToType() : MessageType.Info;
			Show(text, msgType);
		}

		// System Functions
		void Start()
		{
			messageText.SetText("App_EditorStartup");
		}

		void OnEnable()
		{
			Instance = this;
			Application.logMessageReceived += ShowException;
			T3Logger.AddListener("Notice", OnLogNotice);
		}

		void OnDisable()
		{
			Application.logMessageReceived -= ShowException;
			T3Logger.RemoveListener("Notice", OnLogNotice);
		}
	}

	public static class MessageTypeExtension
	{
		public static Color GetColor(this HeaderMessage.MessageType type)
		{
			return type switch
			{
				HeaderMessage.MessageType.Warn => new(1f, 1f, 0.4f, 0.7f),
				HeaderMessage.MessageType.Error => new(1f, 0.4f, 0.4f, 0.7f),
				HeaderMessage.MessageType.Success => new(0.4f, 1f, 0.4f, 0.7f),
				HeaderMessage.MessageType.Info or _ => new(0.4f, 0.7f, 1f, 0.7f),
			};
		}

		public static HeaderMessage.MessageType ToType(this T3LogType type)
		{
			return type switch
			{
				T3LogType.Warn => HeaderMessage.MessageType.Warn,
				T3LogType.Error => HeaderMessage.MessageType.Error,
				T3LogType.Success => HeaderMessage.MessageType.Success,
				T3LogType.Info or _ => HeaderMessage.MessageType.Info,
			};
		}
	}
}