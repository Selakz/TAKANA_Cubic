#nullable enable

using System.ComponentModel;
using App.AutoUpdate.Model;
using Newtonsoft.Json;
using T3Framework.Runtime.WebRequest;

namespace App.AutoUpdate.Schema
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