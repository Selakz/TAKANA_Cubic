#nullable enable

using UnityEngine;

namespace App.AutoUpdate
{
	[CreateAssetMenu(fileName = "Auto Update Config", menuName = "ScriptableObjects/Auto Update Config")]
	public class AutoUpdateConfig : ScriptableObject
	{
		public int timeoutMilliseconds = 0;

		public string versionUrl = string.Empty;

		public string updateUrl = string.Empty;

		public string versionsUrl = string.Empty;

		public string downloadUrl = string.Empty;

		public string updateHelperVersion = string.Empty;
	}
}