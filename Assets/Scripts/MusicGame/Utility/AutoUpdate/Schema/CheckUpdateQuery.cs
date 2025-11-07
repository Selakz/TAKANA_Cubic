#nullable enable

using System.ComponentModel;
using MusicGame.Utility.AutoUpdate.Model;
using Newtonsoft.Json;
using T3Framework.Runtime.WebRequest;

namespace MusicGame.Utility.AutoUpdate.Schema
{
	public struct CheckUpdateQuery : IQueryParam
	{
		public string CurrentVersion { get; set; }

		public string Query => $"current_version={CurrentVersion}";
	}

	public struct CheckUpdateResponse
	{
		[DefaultValue(false)]
		[JsonProperty("has_update")]
		public bool HasUpdate { get; set; }

		[JsonProperty("version_descriptor")]
		public VersionDescriptor VersionDescriptor { get; set; }
	}
}