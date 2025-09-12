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
		[Description("歌曲的唯一标识符")]
		public string Id { get; set; } = string.Empty;

		[Description("展示的曲名")]
		public I18NString Title { get; set; } = new();

		[Description("展示的曲师")]
		public I18NString Composer { get; set; } = new();

		[Description("展示的画师")]
		public I18NString Illustrator { get; set; } = new();

		[Description("展示的BPM信息")]
		public string BpmDisplay { get; set; } = string.Empty;

		[Description("对歌曲的描述")]
		public I18NString Description { get; set; } = new();

		[Description("各难度信息")]
		public Dictionary<int, DifficultyInfo> Difficulties { get; set; } = new();
	}
}