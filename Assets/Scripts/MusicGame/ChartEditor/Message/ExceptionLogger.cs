#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Message
{
	public interface IExceptionLogService
	{
		public NotifiableProperty<int> UnreadExceptionCount { get; }

		public void ClearUnreadCount();

		public IReadOnlyList<ExceptionRecord> GetRecords(ExceptionTimeRange range);

		public string BuildLogText(
			ExceptionTimeRange range, int maxChars, out int recordCount, out bool isTruncated);
	}

	public class ExceptionLogger : HierarchySystem<ExceptionLogger>, IExceptionLogService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		public NotifiableProperty<int> UnreadExceptionCount { get; } = new(0);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, UpdateLogFolder),
			new CustomRegistrar(
				() => Application.logMessageReceived += LogToProject,
				() => Application.logMessageReceived -= LogToProject),
			new CustomRegistrar(
				() => Application.wantsToQuit += OnWantsToQuit,
				() => Application.wantsToQuit -= OnWantsToQuit)
		};

		// Private
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;

		private readonly List<ExceptionRecord> sessionRecords = new();
		private readonly HashSet<string> touchedLogFolders = new();

		private bool isQuitting;
		private bool isLogging;
		private string defaultLogFolderPath = string.Empty;
		private string logFolderPath = string.Empty;

		// Defined Functions
		public void ClearUnreadCount() => UnreadExceptionCount.Value = 0;

		public IReadOnlyList<ExceptionRecord> GetRecords(ExceptionTimeRange range)
		{
			if (range == ExceptionTimeRange.CurrentSession)
			{
				return sessionRecords.ToArray();
			}

			DateTime now = DateTime.Now;
			DateTime cutoff = range switch
			{
				ExceptionTimeRange.Last15Minutes => now.AddMinutes(-15),
				ExceptionTimeRange.Last1Hour => now.AddHours(-1),
				ExceptionTimeRange.Today => now.Date,
				_ => DateTime.MinValue
			};

			List<ExceptionRecord> merged = new();
			HashSet<(long Seconds, LogType Type, string Message)> seen = new();

			foreach (var record in sessionRecords)
			{
				if (record.Timestamp < cutoff) continue;
				merged.Add(record);
				seen.Add(CreateKey(record));
			}

			for (DateTime date = cutoff.Date; date <= now.Date; date = date.AddDays(1))
			{
				string fileName = $"{date:yyyy-MM-dd} - Exceptions.log";
				foreach (string folder in touchedLogFolders)
				{
					string filePath = Path.Combine(folder, fileName);
					if (!File.Exists(filePath)) continue;
					foreach (var record in ParseLogFile(filePath, cutoff))
					{
						if (seen.Add(CreateKey(record)))
						{
							merged.Add(record);
						}
					}
				}
			}

			merged.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
			return merged;

			static (long Seconds, LogType Type, string Message) CreateKey(ExceptionRecord record)
			{
				string normalizedMessage = record.Message.Replace("\r\n", "\n").Trim();
				return (record.Timestamp.Ticks / TimeSpan.TicksPerSecond, record.LogType, normalizedMessage);
			}

			static List<ExceptionRecord> ParseLogFile(string filePath, DateTime minTimestamp)
			{
				List<ExceptionRecord> records = new();
				string[] lines;
				try
				{
					lines = File.ReadAllLines(filePath);
				}
				catch
				{
					return records;
				}

				const string timePrefix = "[time]: ";
				const string typePrefix = "[type]: ";
				const string messagePrefix = "[exception message]: ";
				const string stackPrefix = "[stack trace]: ";

				bool hasActive = false;
				DateTime currentTime = default;
				LogType currentType = LogType.Exception;
				StringBuilder messageBuilder = new();
				StringBuilder stackBuilder = new();
				int activeField = 0;

				foreach (string line in lines)
				{
					if (line.StartsWith(timePrefix, StringComparison.Ordinal))
					{
						FlushCurrent();
						hasActive = TryParseTimestamp(line[timePrefix.Length..], out currentTime);
						currentType = LogType.Exception;
						messageBuilder.Clear();
						stackBuilder.Clear();
						activeField = 0;
						continue;
					}

					if (!hasActive) continue;

					if (line.StartsWith(typePrefix, StringComparison.Ordinal))
					{
						Enum.TryParse(line[typePrefix.Length..].Trim(), out currentType);
						activeField = 0;
					}
					else if (line.StartsWith(messagePrefix, StringComparison.Ordinal))
					{
						activeField = 1;
						messageBuilder.Append(line[messagePrefix.Length..]);
					}
					else if (line.StartsWith(stackPrefix, StringComparison.Ordinal))
					{
						activeField = 2;
						stackBuilder.Append(line[stackPrefix.Length..]);
					}
					else if (activeField == 1)
					{
						messageBuilder.AppendLine().Append(line);
					}
					else if (activeField == 2)
					{
						stackBuilder.AppendLine().Append(line);
					}
				}

				FlushCurrent();
				return records;

				void FlushCurrent()
				{
					if (!hasActive || currentTime < minTimestamp) return;
					records.Add(new ExceptionRecord(
						currentTime,
						currentType,
						messageBuilder.ToString().TrimEnd('\r', '\n'),
						stackBuilder.ToString().TrimEnd('\r', '\n')));
				}

				static bool TryParseTimestamp(string raw, out DateTime timestamp)
				{
					string trimmed = raw.Trim();
					return DateTime.TryParseExact(
						       trimmed,
						       "yyyy-MM-dd HH:mm:ss",
						       CultureInfo.InvariantCulture,
						       DateTimeStyles.AssumeLocal,
						       out timestamp) ||
					       DateTime.TryParse(
						       trimmed,
						       CultureInfo.InvariantCulture,
						       DateTimeStyles.AssumeLocal,
						       out timestamp) ||
					       DateTime.TryParse(
						       trimmed,
						       CultureInfo.CurrentCulture,
						       DateTimeStyles.AssumeLocal,
						       out timestamp);
				}
			}
		}

		public string BuildLogText(
			ExceptionTimeRange range, int maxChars, out int recordCount, out bool isTruncated)
		{
			var records = GetRecords(range);
			recordCount = records.Count;
			isTruncated = false;

			if (records.Count == 0)
			{
				return string.Empty;
			}

			List<string> selectedBlocks = new();
			int remainingChars = Math.Max(0, maxChars);

			for (int i = records.Count - 1; i >= 0; i--)
			{
				string block = records[i].ToLogBlock();
				int needed = block.Length + Environment.NewLine.Length;
				if (selectedBlocks.Count > 0 && needed > remainingChars)
				{
					isTruncated = true;
					break;
				}

				if (block.Length > remainingChars && remainingChars > 0)
				{
					selectedBlocks.Add(block[^remainingChars..]);
					isTruncated = true;
					break;
				}

				selectedBlocks.Add(block);
				remainingChars -= needed;
			}

			selectedBlocks.Reverse();
			StringBuilder result = new();
			if (isTruncated)
			{
				result.AppendLine("[truncated]: Older exception entries were omitted due to size limit.");
				result.AppendLine();
			}

			foreach (string block in selectedBlocks)
			{
				result.AppendLine(block);
			}

			return result.ToString();
		}

		private void UpdateLogFolder(LevelInfo? info)
		{
			if (info is not null && Path.IsPathFullyQualified(info.LevelPath))
			{
				logFolderPath = FileHelper.GetAbsolutePathFromRelative(info.LevelPath, "Logs");
			}
			else
			{
				logFolderPath = defaultLogFolderPath;
			}

			touchedLogFolders.Add(logFolderPath);
		}

		private void LogToProject(string condition, string stackTrace, LogType logType)
		{
			if (isQuitting || isLogging) return;
			if (logType is not (LogType.Error or LogType.Exception or LogType.Assert)) return;

			isLogging = true;
			try
			{
				DateTime now = DateTime.Now;
				ExceptionRecord record = new(now, logType, condition, stackTrace);
				sessionRecords.Add(record);
				UnreadExceptionCount.Value++;

				string logFilePath = Path.Combine(logFolderPath, $"{now:yyyy-MM-dd} - Exceptions.log");
				Directory.CreateDirectory(logFolderPath);
				File.AppendAllLines(logFilePath, new[]
				{
					$"[time]: {now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}",
					$"[type]: {logType}",
					$"[exception message]: {condition}",
					$"[stack trace]: {stackTrace}",
					string.Empty
				});
			}
			finally
			{
				isLogging = false;
			}
		}

		private bool OnWantsToQuit()
		{
			isQuitting = true;
			return true;
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			defaultLogFolderPath = Path.Combine(Application.streamingAssetsPath, "Logs");
			logFolderPath = defaultLogFolderPath;
			touchedLogFolders.Add(logFolderPath);
		}
	}
}
