using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Message
{
	public class HeaderMessage : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Image panelImage;
		[SerializeField] private TMP_Text messageText;
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
			Instance.messageText.text = message;
			Instance.animator.Play(0);
			Debug.Log($"HeaderMessage Show: {message}");
		}

		// Event Handlers
		private void ShowException(string condition, string stackTrace, LogType logType)
		{
			if (logType is LogType.Error or LogType.Exception or LogType.Assert)
				Show("出现未处理异常，请将日志汇报给作者", MessageType.Error);
		}

		// System Functions
		void OnEnable()
		{
			Instance = this;
			Application.logMessageReceived += ShowException;
		}

		void OnDisable()
		{
			Application.logMessageReceived -= ShowException;
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
	}
}