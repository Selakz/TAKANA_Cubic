#nullable enable

using T3Framework.Runtime.WebRequest;

namespace MusicGame.Utility.AutoUpdate.Schema
{
	public struct DownloadPackQuery : IQueryParam
	{
		public string Version { get; set; }

		public string Query => $"version={Version}";
	}
}