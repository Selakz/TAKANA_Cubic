#nullable enable

using MusicGame.Utility.AutoUpdate.Model;
using Newtonsoft.Json;
using T3Framework.Runtime.WebRequest;

namespace MusicGame.Utility.AutoUpdate.Schema
{
	public struct VersionQuery : IQueryParam
	{
		public string Version { get; set; }

		public string Query => $"version={Version}";
	}

	public struct VersionResponse
	{
		[JsonProperty("version_descriptor")]
		public VersionDescriptor VersionDescriptor { get; set; }
	}
}