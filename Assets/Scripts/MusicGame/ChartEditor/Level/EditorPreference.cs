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
		[Description("初始默认选择的难度 | 1 ~ 5 依次为 Normal ~ Ravage")]
		public int Difficulty { get; set; } = 3;

		[Description("谱面时间偏移量")]
		public int Offset { get; set; } = 0;

		[Description("音乐音量大小百分比（整数） | 范围 [0, 100]")]
		public int MusicVolumePercent { get; set; } = 100;

		[Description("初始默认设置的速度 | 范围 [0, 10]")]
		public float Speed { get; set; }

		[Description("横向时间格线密度 | 范围 [0, ?)")]
		public int TimeGridLineCount { get; set; } = 4;

		[Description("纵向位置格线密度 | 范围 [0, ?)")]
		public float WidthGridInterval { get; set; } = 1.5f;

		[Description("纵向位置格线偏移")]
		public float WidthGridOffset { get; set; } = 0f;

		[Description("BPM列表，键为时间，值为BPM值")]
		public Dictionary<T3Time, float> BpmList { get; set; } = new();
	}
}