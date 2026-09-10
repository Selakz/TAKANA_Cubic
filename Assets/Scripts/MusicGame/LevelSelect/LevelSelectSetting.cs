#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.LevelSelect
{
	[Description("Header")]
	public class LevelSelectSetting : ISingletonSetting<LevelSelectSetting>
	{
		[Description("LastSongId")]
		public NotifiableProperty<string> LastSongId { get; set; } = new(string.Empty);

		[Description("LastLevelPath")]
		public NotifiableProperty<string> LastLevelPath { get; set; } = new(string.Empty);

		[Description("LastDifficulty")]
		public NotifiableProperty<int> LastDifficulty { get; set; } = new(3);

		[Description("LastPackId")]
		public NotifiableProperty<string> LastPackId { get; set; } = new(string.Empty);

		[Description("LastSortId")]
		public NotifiableProperty<string> LastSortId { get; set; } = new(string.Empty);

		[Description("LastSortAscend")]
		public NotifiableProperty<bool> LastSortAscend { get; set; } = new(true);
	}
}
