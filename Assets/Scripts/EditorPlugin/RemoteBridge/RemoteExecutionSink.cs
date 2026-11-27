#nullable enable

using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.RemoteBridge
{
	public sealed class RemoteExecutionSink
	{
		private const int MaxLogEntries = 64;
		private const int MaxLogMessageLength = 2048;

		private readonly List<RemoteLogEntry> logs = new();
		private readonly Action<bool, string?> resolveCallback;
		private readonly Action<string, string?> rejectCallback;
		private bool settled;

		public bool IsSettled => settled;

		public IReadOnlyList<RemoteLogEntry> Logs => logs;

		public RemoteExecutionSink(Action<bool, string?> resolveCallback, Action<string, string?> rejectCallback)
		{
			this.resolveCallback = resolveCallback;
			this.rejectCallback = rejectCallback;
		}

		public void onLog(string level, string message)
		{
			if (settled || logs.Count >= MaxLogEntries) return;
			string safeMessage = message.Length > MaxLogMessageLength
				? message[..MaxLogMessageLength]
				: message;
			logs.Add(new RemoteLogEntry
			{
				Level = string.IsNullOrEmpty(level) ? "log" : level,
				Message = safeMessage
			});
		}

		public void onResolve(bool hasValue, string? jsonValue)
		{
			if (settled) return;
			settled = true;
			resolveCallback.Invoke(hasValue, jsonValue);
		}

		public void onReject(string message, string? stack)
		{
			if (settled) return;
			settled = true;
			rejectCallback.Invoke(message, stack);
		}

		public bool TryForceSettle()
		{
			if (settled) return false;
			settled = true;
			return true;
		}
	}
}
