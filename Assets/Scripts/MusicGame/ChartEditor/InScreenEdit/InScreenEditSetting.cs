using System;
using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class InScreenEditSetting : ISingletonSetting<InScreenEditSetting>
	{
		[Description("�������ʱ�Ƿ�ʹ����ĳ�ʼ����ʱ��Ϊ��������ʱ��")]
		public bool IsInitialTrackLengthToEnd { get; set; } = true;

		[Description("����һ��Ϊfalse�������ĳ�ʼ���� | ��λ������")]
		public T3Time InitialTrackLength { get; set; } = 3000;

		[Description("�����λ��������ĺ������ʱ���С�ڸ�ֵʱ�����������ø����� | ��λ������")]
		public T3Time TimeSnapDistance { get; set; } = 1000;

		[Description("�����λ���������������߾����С�ڸ�ֵʱ�����������ø�����")]
		public float WidthSnapDistance { get; set; } = 0.5f;

		[Description("���� W �� S ʱ Note �ƶ���ʱ�䳤�� | ��λ������")]
		public T3Time NoteNudgeDistance { get; set; } = 10;

		[Description("�ķ��������ģ��ĺ��������ɫ | �ĸ�ֵ������ɫ��RGBA����Χ(0, 1)������ʡ�����Ż�Alphaֵ")]
		public Color BeatColor { get; set; } = new(0.6f, 0, 0, 0.9f);

		[Description("�˷������ĺ��������ɫ")]
		public Color QuaverColor { get; set; } = new(0.7f, 0.6f, 0, 0.9f);

		[Description("ʮ���������ĺ��������ɫ")]
		public Color SemiQuaverColor { get; set; } = new(0.7f, 0.6f, 0, 0.9f);

		[Description("ʮ���������ĺ��������ɫ")]
		public Color TripletColor { get; set; } = new(0.7f, 0.5f, 0.7f, 0.9f);

		[Description("�������������ʱ�ĺ��������ɫ")]
		public Color DefaultColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.9f);
	}
}