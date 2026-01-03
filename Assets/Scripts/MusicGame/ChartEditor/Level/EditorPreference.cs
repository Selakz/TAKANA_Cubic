#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.Level
{
	[SettingFileName("preference.yaml")]
	public class EditorPreference : ISetting<EditorPreference>, IPreference
	{
		[Description("Difficulty")]
		public int Difficulty { get; set; } = 3;

		[Description("Offset")]
		public int Offset { get; set; } = 0;

		[Description("MusicVolumePercent")]
		public int MusicVolumePercent { get; set; } = 100;

		[Description("Speed")]
		public float Speed { get; set; }

		[Description("TimeGridLineCount")]
		public int TimeGridLineCount { get; set; } = 4;

		[Description("WidthGridInterval")]
		public float WidthGridInterval { get; set; } = 1.5f;

		[Description("WidthGridOffset")]
		public float WidthGridOffset { get; set; } = 0f;

		[Description("BpmList")]
		public Dictionary<T3Time, float> BpmList { get; set; } = new();
	}
}