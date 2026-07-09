#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace MusicGame.Utility.Deemo
{
	[JsonObject(MemberSerialization.OptIn)]
	public class DeemoChart
	{
		[JsonProperty("speed")]
		public float Speed { get; set; }

		[JsonProperty("notes")]
		public List<DeemoNote> Notes { get; set; } = new();

		[JsonProperty("links")]
		public List<DeemoLink> Links { get; set; } = new();
	}

	[JsonObject(MemberSerialization.OptIn, IsReference = true)]
	public class DeemoNote
	{
		[JsonProperty("_time")]
		public float Time { get; set; }

		[JsonProperty("pos")]
		public float Position { get; set; }

		[JsonProperty("size")]
		public float Size { get; set; } = 1f;

		[JsonProperty("duration")]
		public float Duration { get; set; }

		[JsonProperty("swipe")]
		public bool IsSwipe { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public class DeemoLink
	{
		[JsonProperty("notes")]
		public List<DeemoNote> Notes { get; set; } = new();
	}
}