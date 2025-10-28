using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.Gameplay.Level
{
	[Description("��ϷUI����")]
	public class PlayfieldSetting : ISingletonSetting<PlayfieldSetting>
	{
		[Description("Tap��Hold�����߽�ļ����С")]
		public NotifiableProperty<float> TrackGap1 { get; set; } = new(0.15f);

		[Description("Slide�����߽�ļ����С")]
		public NotifiableProperty<float> TrackGap2 { get; set; } = new(0.3f);

		[Description("���Ĭ����ɫ������Ҳ�������ù����Ĭ��͸����")]
		public NotifiableProperty<Color> TrackFaceDefaultColor { get; set; } = new(Color.black);

		[Description("Note����ʱ���δ���ж���������ͼ���������ж�ʱ��֮���ӳ����ٵ�ʱ�䣨�Ǳ�Ҫ�����޸ģ�")]
		public NotifiableProperty<int> TimeAfterEnd { get; set; } = new(2000);

		[Description("��Note��1�����״�λ�ڸø߶�����ʱ�Ż�������������ͼ�����ݣ��Ǳ�Ҫ�����޸ģ�")]
		public NotifiableProperty<float> UpperThreshold { get; set; } = new(8);

		[Description("��Note��1�����״�λ�ڸø߶�����ʱ�Ż�������������ͼ�����ݣ��Ǳ�Ҫ�����޸ģ�")]
		public NotifiableProperty<float> LowerThreshold { get; set; } = new(-5);
	}
}