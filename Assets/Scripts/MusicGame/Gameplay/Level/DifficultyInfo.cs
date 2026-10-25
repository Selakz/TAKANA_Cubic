#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.I18N;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	public class DifficultyInfo : ISetting<DifficultyInfo>
	{
		[Description("LevelDisplay")]
		public string LevelDisplay { get; set; } = "00";

		[Description("Charter")]
		public I18NString Charter { get; set; } = new();

		[Description("IsCustom")]
		public bool IsCustom { get; set; }

		[Description("Name")]
		public string Name { get; set; } = string.Empty;

		[Description("Color")]
		public Color Color { get; set; } = Color.white;
	}
}
