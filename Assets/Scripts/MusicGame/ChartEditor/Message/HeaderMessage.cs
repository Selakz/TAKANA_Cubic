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
		public static void Show(MessageType type, string key, params string[] args)
		{
			Instance.panelImage.color = type.GetColor();
			Instance.messageText.SetText(key, args);
			Instance.animator.Play(0);
			Debug.Log($"HeaderMessage Show: {Instance.messageText.Text.text}");
		}

		public static void ShowRaw(MessageType type, string text)
		{
			Instance.panelImage.color = type.GetColor();
			Instance.messageText.Text.text = text;
			Instance.animator.Play(0);
			Debug.Log($"HeaderMessage ShowRaw: {text}");
		}

		// Event Handlers
		private void ShowException(string condition, string stackTrace, LogType logType)
		{
			if (logType is LogType.Error or LogType.Exception or LogType.Assert)
				Show(MessageType.Error, "App_Exception");
		}

		private static void OnLogNotice(string message, Enum type)
		{
			var split = message.Split('|');
			MessageType msgType = type is T3LogType t ? t.ToType() : MessageType.Info;
			Show(msgType, split[0], split[1..]);
		}

		private static void OnLogNoticeRaw(string message, Enum type)
		{
			MessageType msgType = type is T3LogType t ? t.ToType() : MessageType.Info;
			ShowRaw(msgType, message);
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
			T3Logger.AddListener("NoticeRaw", OnLogNoticeRaw);
		}

		void OnDisable()
		{
			Application.logMessageReceived -= ShowException;
			T3Logger.RemoveListener("Notice", OnLogNotice);
			T3Logger.RemoveListener("NoticeRaw", OnLogNoticeRaw);
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