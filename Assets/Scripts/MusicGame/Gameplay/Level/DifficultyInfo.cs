#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Localization;
using T3Framework.Runtime.Setting;

namespace MusicGame.Gameplay.Level
{
	public class DifficultyInfo : ISetting<DifficultyInfo>
	{
		[Description("LevelDisplay")]
		public string LevelDisplay { get; set; } = "00";

		[Description("Charter")]
		public I18NString Charter { get; set; } = new();
	}
}