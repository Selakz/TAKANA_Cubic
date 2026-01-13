#nullable enable

using App.AutoUpdate.Model;
using Newtonsoft.Json;
using T3Framework.Runtime.WebRequest;

namespace App.AutoUpdate.Schema
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