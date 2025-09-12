using System.ComponentModel;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	public class PlayfieldSetting : ISingletonSetting<PlayfieldSetting>
	{
		[Description("��ʼ�ٶ�")]
		public float Speed { get; set; } = 1.0f;

		[Description("��Note��1�����״�λ�ڸø߶�����ʱ�Ż�������������ͼ�����ݣ��Ǳ�Ҫ�����޸ģ�")]
		public float UpperThreshold { get; set; } = 8;

		[Description("��Note��1�����״�λ�ڸø߶�����ʱ�Ż�������������ͼ�����ݣ��Ǳ�Ҫ�����޸ģ�")]
		public float LowerThreshold { get; set; } = -5;

		[Description("Tap��Hold�����߽�ļ����С")]
		public float TrackGap1 { get; set; } = 0.15f;

		[Description("Slide�����߽�ļ����С")]
		public float TrackGap2 { get; set; } = 0.3f;

		[Description("Note����ʱ���δ���ж���������ͼ���������ж�ʱ��֮���ӳ����ٵ�ʱ�䣨�Ǳ�Ҫ�����޸ģ�")]
		public int TimeAfterEnd { get; set; } = 2000;

		[Description("���Ĭ����ɫ������Ҳ�������ù����Ĭ��͸����")]
		public Color TrackFaceDefaultColor { get; set; } = Color.black;
	}
}