#nullable enable

using System;
using Newtonsoft.Json;

namespace MusicGame.Utility.AutoUpdate.Model
{
	public struct VersionDescriptor : IEquatable<VersionDescriptor>
	{
		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("update_log")]
		public string[] UpdateLog { get; set; }

		[JsonProperty("update_date")]
		public string UpdateDate { get; set; }

		public bool Equals(VersionDescriptor other)
		{
			return Version == other.Version && Description == other.Description && UpdateLog.Equals(other.UpdateLog) &&
			       UpdateDate == other.UpdateDate;
		}

		public override bool Equals(object? obj)
		{
			return obj is VersionDescriptor other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Version, Description, UpdateLog, UpdateDate);
		}
	}
}