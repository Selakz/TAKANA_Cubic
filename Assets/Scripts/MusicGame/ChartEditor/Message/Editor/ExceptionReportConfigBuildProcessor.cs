#nullable enable
#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MusicGame.ChartEditor.Message.Editor
{
	public class ExceptionReportConfigBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
	{
		private sealed class ConfigBackup
		{
			public ExceptionReportConfig Config { get; }
			public string ReportUrl { get; }
			public string AuthHeaderName { get; }
			public string AuthHeaderValue { get; }

			public ConfigBackup(
				ExceptionReportConfig config,
				string reportUrl,
				string authHeaderName,
				string authHeaderValue)
			{
				Config = config;
				ReportUrl = reportUrl;
				AuthHeaderName = authHeaderName;
				AuthHeaderValue = authHeaderValue;
			}
		}

		private const string ReportUrlEnvVar = "TAKANA_REPORT_URL";
		private const string ReportTokenEnvVar = "TAKANA_REPORT_TOKEN";
		private const string AuthHeaderNameEnvVar = "TAKANA_REPORT_AUTH_HEADER_NAME";
		private const string DefaultAuthHeaderName = "Authorization";

		private static readonly List<ConfigBackup> Backups = new();

		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			Backups.Clear();

			string? reportUrl = Environment.GetEnvironmentVariable(ReportUrlEnvVar)?.Trim();
			string? reportToken = Environment.GetEnvironmentVariable(ReportTokenEnvVar)?.Trim();
			if (string.IsNullOrEmpty(reportUrl) && string.IsNullOrEmpty(reportToken))
			{
				Debug.Log(
					$"[ExceptionReportConfigBuildProcessor] {ReportUrlEnvVar} and {ReportTokenEnvVar} are not set; skipping injection.");
				return;
			}

			string? customHeaderName = Environment.GetEnvironmentVariable(AuthHeaderNameEnvVar)?.Trim();
			string headerName = string.IsNullOrEmpty(customHeaderName) ? DefaultAuthHeaderName : customHeaderName!;

			string[] guids = AssetDatabase.FindAssets($"t:{nameof(ExceptionReportConfig)}");
			if (guids.Length == 0)
			{
				Debug.LogWarning("[ExceptionReportConfigBuildProcessor] No ExceptionReportConfig asset found.");
				return;
			}

			foreach (string guid in guids)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(guid);
				ExceptionReportConfig? config = AssetDatabase.LoadAssetAtPath<ExceptionReportConfig>(assetPath);
				if (config is null) continue;

				Backups.Add(new ConfigBackup(config, config.reportUrl, config.authHeaderName, config.authHeaderValue));

				if (!string.IsNullOrEmpty(reportUrl)) config.reportUrl = reportUrl!;
				if (!string.IsNullOrEmpty(reportToken))
				{
					config.authHeaderName = headerName;
					config.authHeaderValue = FormatHeaderValue(headerName, reportToken!);
				}

				EditorUtility.SetDirty(config);
			}

			AssetDatabase.SaveAssets();
			Debug.Log($"[ExceptionReportConfigBuildProcessor] Injected config into {Backups.Count} asset(s).");
			return;

			static string FormatHeaderValue(string header, string token)
			{
				if (string.Equals(header, DefaultAuthHeaderName, StringComparison.OrdinalIgnoreCase) &&
				    !token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
				{
					return $"Bearer {token}";
				}

				return token;
			}
		}

		public void OnPostprocessBuild(BuildReport report)
		{
			if (Backups.Count == 0) return;

			foreach (ConfigBackup backup in Backups)
			{
				if (backup.Config == null) continue;
				backup.Config.reportUrl = backup.ReportUrl;
				backup.Config.authHeaderName = backup.AuthHeaderName;
				backup.Config.authHeaderValue = backup.AuthHeaderValue;
				EditorUtility.SetDirty(backup.Config);
			}

			AssetDatabase.SaveAssets();
			Debug.Log($"[ExceptionReportConfigBuildProcessor] Restored {Backups.Count} asset(s) after build.");
			Backups.Clear();
		}
	}
}

#endif