#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Semver;
using UnityEngine;

namespace EditorPlugin.EditorIntegration
{
	[CreateAssetMenu(fileName = "PluginApiVersionConfig", menuName = "T3EditorConfig/PluginApiVersionConfig")]
	public class PluginApiVersionConfig : ScriptableObject
	{
		public List<ApiVersionEntry> entries = new();

		public SemVersion? GetApiVersion(string appVersion)
		{
			if (string.IsNullOrEmpty(appVersion) || !SemVersion.TryParse(appVersion, out var current)) return null;
			if (!TryGetCurrentEntry(current, out var entry)) return null;
			return SemVersion.TryParse(entry.apiVersion, out var api) ? api : null;
		}

		public bool IsApiVersionCompatible(string pluginApiVersion, string appVersion)
		{
			if (string.IsNullOrEmpty(appVersion) || !SemVersion.TryParse(appVersion, out var current)) return false;
			if (!TryGetCurrentEntry(current, out var entry)) return false;
			if (string.IsNullOrEmpty(pluginApiVersion) || !SemVersion.TryParse(pluginApiVersion, out var pluginApi))
			{
				return false;
			}

			if (!SemVersion.TryParse(entry.apiVersion, out var currentApi)) return false;
			if (pluginApi.CompareSortOrderTo(currentApi) > 0) return false;

			if (!SemVersion.TryParse(entry.minCompatibleApiVersion, out var minCompatible)) return false;
			if (pluginApi.CompareSortOrderTo(minCompatible) < 0) return false;

			return true;
		}

		private bool TryGetCurrentEntry(SemVersion appVersion, [NotNullWhen(true)] out ApiVersionEntry? entry)
		{
			ApiVersionEntry? best = null;
			SemVersion? bestAppVersion = null;
			foreach (var candidate in entries)
			{
				if (!SemVersion.TryParse(candidate.appVersion, out var candidateApp)) continue;
				if (candidateApp.CompareSortOrderTo(appVersion) > 0) continue;
				if (bestAppVersion is null || candidateApp.CompareSortOrderTo(bestAppVersion) > 0)
				{
					best = candidate;
					bestAppVersion = candidateApp;
				}
			}

			entry = best;
			return best is not null;
		}
	}

	[Serializable]
	public class ApiVersionEntry
	{
		public string appVersion = string.Empty;

		public string apiVersion = string.Empty;

		public string minCompatibleApiVersion = string.Empty;
	}
}