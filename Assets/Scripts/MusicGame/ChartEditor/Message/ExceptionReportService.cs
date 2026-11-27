#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Level;
using Newtonsoft.Json;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Runtime.WebRequest;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Message
{
	public readonly struct ExceptionReportResult
	{
		public bool IsSuccess { get; }
		public string? ErrorMessage { get; }
		public bool ExcludedAudio { get; }
		public bool ExcludedCover { get; }
		public bool SkippedProjectDueToSize { get; }
		public int MaxArchiveSizeMb { get; }

		public ExceptionReportResult(
			bool isSuccess,
			string? errorMessage = null,
			bool excludedAudio = false,
			bool excludedCover = false,
			bool skippedProjectDueToSize = false,
			int maxArchiveSizeMb = 0)
		{
			IsSuccess = isSuccess;
			ErrorMessage = errorMessage;
			ExcludedAudio = excludedAudio;
			ExcludedCover = excludedCover;
			SkippedProjectDueToSize = skippedProjectDueToSize;
			MaxArchiveSizeMb = maxArchiveSizeMb;
		}
	}

	public interface IExceptionReportService
	{
		public UniTask<ExceptionReportResult> SubmitReportAsync(
			string reporterName,
			string summary,
			ExceptionTimeRange timeRange,
			bool includeProject,
			CancellationToken cancellationToken = default);
	}

	public class ExceptionReportService : HierarchySystem<ExceptionReportService>, IExceptionReportService
	{
		// Serializable and Public
		[SerializeField] private ExceptionReportConfig config = default!;

		public override bool AsImplementedInterfaces => true;

		// Private
		[Inject] private readonly NotifiableProperty<LevelInfo?> levelInfo = default!;
		[Inject] private readonly IExceptionLogService exceptionLogService = default!;
		[Inject] private readonly EditorLevelSaver editorLevelSaver = default!;

		// Defined Functions
		public async UniTask<ExceptionReportResult> SubmitReportAsync(
			string reporterName,
			string summary,
			ExceptionTimeRange timeRange,
			bool includeProject,
			CancellationToken cancellationToken = default)
		{
			(string reportUrl, string authHeaderName, string authHeaderValue) = ResolveEndpointConfig();
			if (string.IsNullOrWhiteSpace(reportUrl))
			{
				return new ExceptionReportResult(false, I18NSystem.GetText("ExceptionReport_ConfigMissing"));
			}

			var client = WebRequestManager.Instance.Client;
			if (client is null) return new ExceptionReportResult(false, "HttpClient is not initialized.");

			string logText = exceptionLogService.BuildLogText(
				timeRange, config.maxLogChars, out int recordCount, out bool isTruncated);

			LevelInfo? currentLevel = levelInfo.Value;
			bool hasProject = currentLevel is not null;
			string projectSummary = BuildProjectSummary(currentLevel);

			List<string> tempFilesToCleanup = new();
			string? archivePath = null;
			string archiveFileName = "project.t3pkg";
			bool excludedAudio = false;
			bool excludedCover = false;
			bool skippedDueToSize = false;

			try
			{
				if (includeProject && currentLevel is not null)
				{
					var archiveResult = await CreateProjectArchiveAsync(currentLevel, tempFilesToCleanup);
					archivePath = archiveResult.ArchivePath;
					archiveFileName = archiveResult.ArchiveFileName;
					excludedAudio = archiveResult.ExcludedAudio;
					excludedCover = archiveResult.ExcludedCover;
					skippedDueToSize = archiveResult.SkippedDueToSize;
				}

				using HttpRequestMessage request = new(HttpMethod.Post, reportUrl);
				MultipartFormDataContent formData = new();
				request.Content = formData;

				formData.Add(new StringContent(reporterName.Trim(), Encoding.UTF8), "reporterName");
				formData.Add(new StringContent(summary.Trim(), Encoding.UTF8), "summary");
				formData.Add(new StringContent(timeRange.ToString(), Encoding.UTF8), "timeRange");
				formData.Add(new StringContent(recordCount.ToString(), Encoding.UTF8), "exceptionCount");
				formData.Add(new StringContent(isTruncated ? "true" : "false", Encoding.UTF8), "logsTruncated");
				formData.Add(new StringContent(Application.version, Encoding.UTF8), "appVersion");
				formData.Add(new StringContent(Application.unityVersion, Encoding.UTF8), "unityVersion");
				formData.Add(new StringContent(SystemInfo.operatingSystem, Encoding.UTF8), "operatingSystem");
				formData.Add(new StringContent(SystemInfo.deviceModel, Encoding.UTF8), "deviceModel");
				formData.Add(new StringContent(DateTimeOffset.Now.ToString("O"), Encoding.UTF8), "submittedAt");
				formData.Add(new StringContent(hasProject ? "true" : "false", Encoding.UTF8), "hasProject");
				formData.Add(new StringContent(archivePath != null ? "true" : "false", Encoding.UTF8), "projectIncluded");
				formData.Add(new StringContent(excludedAudio ? "true" : "false", Encoding.UTF8), "projectExcludedAudio");
				formData.Add(new StringContent(excludedCover ? "true" : "false", Encoding.UTF8), "projectExcludedCover");
				formData.Add(new StringContent(projectSummary, Encoding.UTF8), "projectSummary");

				if (!string.IsNullOrEmpty(logText))
				{
					byte[] logBytes = Encoding.UTF8.GetBytes(logText);
					ByteArrayContent logContent = new(logBytes);
					logContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf-8" };
					string logFileName = $"exceptions_{DateTime.Now:yyyyMMdd_HHmmss}.log";
					formData.Add(logContent, "logFile", logFileName);
				}

				if (archivePath is not null && File.Exists(archivePath))
				{
					FileStream archiveStream = new(
						archivePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
					StreamContent archiveContent = new(archiveStream);
					archiveContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
					formData.Add(archiveContent, "projectFile", archiveFileName);
				}

				if (!string.IsNullOrWhiteSpace(authHeaderName) &&
				    !string.IsNullOrWhiteSpace(authHeaderValue))
				{
					request.Headers.TryAddWithoutValidation(authHeaderName, authHeaderValue);
				}

				using CancellationTokenSource timeoutCts =
					CancellationTokenSource.CreateLinkedTokenSource(
						cancellationToken, this.GetCancellationTokenOnDestroy());
				timeoutCts.CancelAfter(Math.Max(1000, config.timeoutMilliseconds));

				using HttpResponseMessage response = await client.SendAsync(request, timeoutCts.Token);
				if (response.IsSuccessStatusCode)
				{
					return new ExceptionReportResult(
						true,
						null,
						excludedAudio,
						excludedCover,
						skippedDueToSize,
						config.maxProjectArchiveSizeMb);
				}

				string responseBody = await response.Content.ReadAsStringAsync();
				string statusMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
				if (!string.IsNullOrWhiteSpace(responseBody))
				{
					statusMessage = $"{statusMessage}: {responseBody}";
				}

				Debug.LogWarning($"ExceptionReport submit failed: {statusMessage}");
				return new ExceptionReportResult(false, statusMessage);
			}
			catch (OperationCanceledException)
			{
				Debug.LogWarning("ExceptionReport submit timed out or was cancelled.");
				return new ExceptionReportResult(false, "Request timed out or was cancelled.");
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"ExceptionReport submit error: {ex.Message}");
				return new ExceptionReportResult(false, ex.Message);
			}
			finally
			{
				foreach (string tempFile in tempFilesToCleanup)
				{
					try
					{
						if (File.Exists(tempFile)) File.Delete(tempFile);
					}
					catch (Exception cleanupEx)
					{
						Debug.LogWarning($"Failed to delete temp file {tempFile}: {cleanupEx.Message}");
					}
				}
			}

			static string BuildProjectSummary(LevelInfo? info)
			{
				if (info is null) return "{}";
				var payload = new
				{
					levelPath = info.LevelPath,
					difficulty = info.Difficulty,
					songId = info.SongInfo.Id,
					title = info.SongInfo.Title,
					composer = info.SongInfo.Composer,
					chartComponentCount = info.Chart.Count
				};
				return JsonConvert.SerializeObject(payload, Formatting.Indented);
			}

			(string ReportUrl, string AuthHeaderName, string AuthHeaderValue) ResolveEndpointConfig()
			{
				string resolvedUrl = config.reportUrl;
				string resolvedHeaderName = config.authHeaderName;
				string resolvedHeaderValue = config.authHeaderValue;

#if UNITY_EDITOR
				if (string.IsNullOrWhiteSpace(resolvedUrl))
				{
					string? envUrl = Environment.GetEnvironmentVariable("TAKANA_REPORT_URL")?.Trim();
					if (!string.IsNullOrEmpty(envUrl)) resolvedUrl = envUrl!;
				}

				if (string.IsNullOrWhiteSpace(resolvedHeaderValue))
				{
					string? envToken = Environment.GetEnvironmentVariable("TAKANA_REPORT_TOKEN")?.Trim();
					if (!string.IsNullOrEmpty(envToken))
					{
						if (string.IsNullOrWhiteSpace(resolvedHeaderName))
						{
							string? envHeaderName = Environment.GetEnvironmentVariable("TAKANA_REPORT_AUTH_HEADER_NAME");
							resolvedHeaderName = string.IsNullOrEmpty(envHeaderName) ? "Authorization" : envHeaderName;
						}

						resolvedHeaderValue = string.Equals(resolvedHeaderName, "Authorization")
							? $"Bearer {envToken}"
							: envToken!;
					}
				}
#endif

				return (resolvedUrl, resolvedHeaderName, resolvedHeaderValue);
			}
		}

		private async UniTask<(
				string? ArchivePath, string ArchiveFileName, bool ExcludedAudio, bool ExcludedCover, bool SkippedDueToSize)>
			CreateProjectArchiveAsync(LevelInfo info, List<string> tempFilesToCleanup)
		{
			string? projectDir = Path.IsPathFullyQualified(info.LevelPath)
				? Path.GetDirectoryName(info.LevelPath)
				: null;

			T3ProjSetting projectSetting = projectDir is not null && File.Exists(info.LevelPath)
				? await ISetting<T3ProjSetting>.LoadAsync(info.LevelPath)
				: new T3ProjSetting();

			string projName = Path.GetFileNameWithoutExtension(info.LevelPath);
			string id = info.SongInfo.Id;
			string baseName = string.IsNullOrWhiteSpace(projName)
				? string.IsNullOrWhiteSpace(id) || !FileHelper.IsValidPath(id) ? "project" : id
				: projName;
			string archiveFileName = $"{baseName}.t3pkg";

			string? unsavedChartTempPath;
			string chartFileName = projectSetting.GetChartFileName(info.Difficulty);
			try
			{
				unsavedChartTempPath = Path.Combine(
					Path.GetTempPath(), $"takana_unsaved_{Guid.NewGuid():N}.editing.json");
				tempFilesToCleanup.Add(unsavedChartTempPath);
				await editorLevelSaver.SaveEditorChartAsync(unsavedChartTempPath);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Failed to snapshot in-memory chart: {ex.Message}");
				unsavedChartTempPath = null;
			}

			string songInfoSnapshot = string.Empty;
			try
			{
				songInfoSnapshot = ISetting<SongInfo>.ToString(info.SongInfo);
			}
			catch (Exception ex)
			{
				Debug.LogWarning($"Failed to snapshot SongInfo: {ex.Message}");
			}

			string zipPath = Path.Combine(Path.GetTempPath(), $"takana_report_{Guid.NewGuid():N}.zip");
			tempFilesToCleanup.Add(zipPath);

			long maxBytes = Math.Max(1, config.maxProjectArchiveSizeMb) * 1024L * 1024L;
			string musicRel = NormalizeRelativePath(projectSetting.MusicFileName);
			string coverRel = NormalizeRelativePath(projectSetting.CoverFileName);

			return await Task.Run(() =>
			{
				bool excludedAudio = false;
				bool excludedCover = false;

				BuildZip(zipPath, false, false);
				if (new FileInfo(zipPath).Length > maxBytes)
				{
					excludedAudio = true;
					BuildZip(zipPath, true, false);
				}

				if (new FileInfo(zipPath).Length > maxBytes)
				{
					excludedCover = true;
					BuildZip(zipPath, true, true);
				}

				if (new FileInfo(zipPath).Length > maxBytes)
				{
					return ((string?)null, archiveFileName, excludedAudio, excludedCover, true);
				}

				return (zipPath, archiveFileName, excludedAudio, excludedCover, false);

				void BuildZip(string targetZipPath, bool skipAudio, bool skipCover)
				{
					if (File.Exists(targetZipPath)) File.Delete(targetZipPath);
					using FileStream zipStream = File.Create(targetZipPath);
					using ZipArchive archive = new(zipStream, ZipArchiveMode.Create);

					if (projectDir is not null && Directory.Exists(projectDir))
					{
						foreach (string file in Directory.EnumerateFiles(projectDir, "*", SearchOption.AllDirectories))
						{
							string relativePath = Path.GetRelativePath(projectDir, file);
							string normalized = NormalizeRelativePath(relativePath);

							if (IsIgnoredProjectEntry(normalized)) continue;
							if (skipAudio && !string.IsNullOrEmpty(musicRel) &&
							    string.Equals(normalized, musicRel, StringComparison.OrdinalIgnoreCase)) continue;
							if (skipCover && !string.IsNullOrEmpty(coverRel) &&
							    string.Equals(normalized, coverRel, StringComparison.OrdinalIgnoreCase)) continue;

							string entryName = relativePath.Replace('\\', '/');
							archive.CreateEntryFromFile(file, entryName);
						}
					}

					if (unsavedChartTempPath is not null && File.Exists(unsavedChartTempPath))
					{
						archive.CreateEntryFromFile(
							unsavedChartTempPath, $"_unsaved_snapshot/{chartFileName}.editing.json");
					}

					if (!string.IsNullOrEmpty(songInfoSnapshot))
					{
						ZipArchiveEntry songInfoEntry =
							archive.CreateEntry($"_unsaved_snapshot/{projectSetting.SongInfoFileName}");
						using Stream entryStream = songInfoEntry.Open();
						byte[] bytes = Encoding.UTF8.GetBytes(songInfoSnapshot);
						entryStream.Write(bytes, 0, bytes.Length);
					}
				}

				static bool IsIgnoredProjectEntry(string normalizedRelativePath)
				{
					if (normalizedRelativePath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) return true;
					return normalizedRelativePath.StartsWith("autosave/", StringComparison.OrdinalIgnoreCase) ||
					       normalizedRelativePath.StartsWith("logs/", StringComparison.OrdinalIgnoreCase);
				}
			});

			static string NormalizeRelativePath(string path) => path.Replace('\\', '/').TrimStart('.', '/');
		}
	}
}