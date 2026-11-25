#nullable enable

using UnityEngine;

namespace MusicGame.ChartEditor.Message
{
	[CreateAssetMenu(fileName = "Exception Report Config", menuName = "ScriptableObjects/Exception Report Config")]
	public class ExceptionReportConfig : ScriptableObject
	{
		public string reportUrl = string.Empty;

		public int timeoutMilliseconds = 30000;

		public string authHeaderName = string.Empty;

		public string authHeaderValue = string.Empty;

		public int maxProjectArchiveSizeMb = 15;

		public int maxLogChars = 500000;
	}
}
