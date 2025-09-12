using System;
using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class InScreenEditSetting : ISingletonSetting<InScreenEditSetting>
	{
		[Description("创建轨道时是否使轨道的初始结束时间为乐曲结束时间")]
		public bool IsInitialTrackLengthToEnd { get; set; } = true;

		[Description("若上一项为false，则轨道的初始长度 | 单位：毫秒")]
		public T3Time InitialTrackLength { get; set; } = 3000;

		[Description("当鼠标位置与最近的横向格线时间差小于该值时，将吸附到该格线上 | 单位：毫秒")]
		public T3Time TimeSnapDistance { get; set; } = 1000;

		[Description("当鼠标位置与最近的纵向格线距离差小于该值时，将吸附到该格线上")]
		public float WidthSnapDistance { get; set; } = 0.5f;

		[Description("单击 W 或 S 时 Note 移动的时间长度 | 单位：毫秒")]
		public T3Time NoteNudgeDistance { get; set; } = 10;

		[Description("四分音符（拍）的横向格线颜色 | 四个值代表颜色的RGBA，范围(0, 1)，请勿省略括号或Alpha值")]
		public Color BeatColor { get; set; } = new(0.6f, 0, 0, 0.9f);

		[Description("八分音符的横向格线颜色")]
		public Color QuaverColor { get; set; } = new(0.7f, 0.6f, 0, 0.9f);

		[Description("十六分音符的横向格线颜色")]
		public Color SemiQuaverColor { get; set; } = new(0.7f, 0.6f, 0, 0.9f);

		[Description("十二分音符的横向格线颜色")]
		public Color TripletColor { get; set; } = new(0.7f, 0.5f, 0.7f, 0.9f);

		[Description("不满足上述情况时的横向格线颜色")]
		public Color DefaultColor { get; set; } = new(0.5f, 0.5f, 0.5f, 0.9f);
	}
}