#nullable enable

using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace MusicGame.ChartEditor.Message
{
	public enum ExceptionTimeRange
	{
		CurrentSession = 0,
		Last15Minutes = 1,
		Last1Hour = 2,
		Today = 3
	}

	public sealed class ExceptionRecord
	{
		public DateTime Timestamp { get; }

		public LogType LogType { get; }

		public string Message { get; }

		public string StackTrace { get; }

		public ExceptionRecord(DateTime timestamp, LogType logType, string message, string stackTrace)
		{
			Timestamp = timestamp;
			LogType = logType;
			Message = message;
			StackTrace = stackTrace;
		}

		public string ToLogBlock()
		{
			StringBuilder sb = new();
			sb.Append("[time]: ").AppendLine(Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
			sb.Append("[type]: ").AppendLine(LogType.ToString());
			sb.Append("[exception message]: ").AppendLine(Message);
			sb.Append("[stack trace]: ").AppendLine(StackTrace);
			return sb.ToString();
		}
	}
}
