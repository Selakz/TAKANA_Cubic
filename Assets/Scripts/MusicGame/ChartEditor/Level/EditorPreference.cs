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
		[Description("��ʼĬ��ѡ����Ѷ� | 1 ~ 5 ����Ϊ Normal ~ Ravage")]
		public int Difficulty { get; set; } = 3;

		[Description("����ʱ��ƫ����")]
		public int Offset { get; set; } = 0;

		[Description("����������С�ٷֱȣ������� | ��Χ [0, 100]")]
		public int MusicVolumePercent { get; set; } = 100;

		[Description("��ʼĬ�����õ��ٶ� | ��Χ [0, 10]")]
		public float Speed { get; set; }

		[Description("����ʱ������ܶ� | ��Χ [0, ?)")]
		public int TimeGridLineCount { get; set; } = 4;

		[Description("����λ�ø����ܶ� | ��Χ [0, ?)")]
		public float WidthGridInterval { get; set; } = 1.5f;

		[Description("����λ�ø���ƫ��")]
		public float WidthGridOffset { get; set; } = 0f;

		[Description("BPM�б���Ϊʱ�䣬ֵΪBPMֵ")]
		public Dictionary<T3Time, float> BpmList { get; set; } = new();
	}
}