#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime.Localization;
using T3Framework.Runtime.Setting;

namespace MusicGame.Gameplay.Level
{
	/// <summary> All about a song </summary>
	public class SongInfo : ISetting<SongInfo>
	{
		[Description("Id")]
		public string Id { get; set; } = string.Empty;

		[Description("Title")]
		public I18NString Title { get; set; } = new();

		[Description("Composer")]
		public I18NString Composer { get; set; } = new();

		[Description("Illustrator")]
		public I18NString Illustrator { get; set; } = new();

		[Description("BpmDisplay")]
		public string BpmDisplay { get; set; } = string.Empty;

		[Description("Description")]
		public I18NString Description { get; set; } = new();

		[Description("Difficulties")]
		public Dictionary<int, DifficultyInfo> Difficulties { get; set; } = new();
	}
}