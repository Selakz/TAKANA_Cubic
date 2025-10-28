using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	[Description("�༭������")]
	public class InScreenEditSetting : ISingletonSetting<InScreenEditSetting>
	{
		[Description("�������ʱ�Ƿ�ʹ����ĳ�ʼ����ʱ��Ϊ��������ʱ��")]
		public NotifiableProperty<bool> IsInitialTrackLengthToEnd { get; set; } = new(true);

		[Description("����һ��Ϊfalse�������ĳ�ʼ���� | ��λ������")]
		public NotifiableProperty<T3Time> InitialTrackLength { get; set; } = new(3000);

		[Description("�����λ��������ĺ������ʱ���С�ڸ�ֵʱ�����������ø����� | ��λ������")]
		public NotifiableProperty<T3Time> TimeSnapDistance { get; set; } = new(1000);

		[Description("�����λ���������������߾����С�ڸ�ֵʱ�����������ø�����")]
		public NotifiableProperty<float> WidthSnapDistance { get; set; } = new(0.5f);

		[Description("���� W �� S ʱ Note �ƶ���ʱ�䳤�� | ��λ������")]
		public NotifiableProperty<T3Time> NoteNudgeDistance { get; set; } = new(10);

		[Description("�ķ��������ģ��ĺ��������ɫ | �ĸ�ֵ������ɫ��RGBA����Χ(0, 1)������ʡ�����Ż�Alphaֵ")]
		public NotifiableProperty<Color> BeatColor { get; set; } = new(new(0.6f, 0, 0, 0.9f));

		[Description("�˷������ĺ��������ɫ")]
		public NotifiableProperty<Color> QuaverColor { get; set; } = new(new(0.7f, 0.6f, 0, 0.9f));

		[Description("ʮ���������ĺ��������ɫ")]
		public NotifiableProperty<Color> SemiQuaverColor { get; set; } = new(new(0.7f, 0.6f, 0, 0.9f));

		[Description("ʮ���������ĺ��������ɫ")]
		public NotifiableProperty<Color> TripletColor { get; set; } = new(new(0.7f, 0.5f, 0.7f, 0.9f));

		[Description("�������������ʱ�ĺ��������ɫ")]
		public NotifiableProperty<Color> DefaultColor { get; set; } = new(new(0.5f, 0.5f, 0.5f, 0.9f));
	}
}