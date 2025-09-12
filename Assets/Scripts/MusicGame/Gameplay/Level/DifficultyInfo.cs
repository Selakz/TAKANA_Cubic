#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Localization;
using T3Framework.Runtime.Setting;

namespace MusicGame.Gameplay.Level
{
	public class DifficultyInfo : ISetting<DifficultyInfo>
	{
		[Description("展示的难度等级文字")]
		public string LevelDisplay { get; set; } = "00";

		[Description("展示的曲师信息")]
		public I18NString Charter { get; set; } = new();
	}
}