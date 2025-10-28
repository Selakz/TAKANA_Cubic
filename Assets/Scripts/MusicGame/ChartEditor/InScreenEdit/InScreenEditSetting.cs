using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	[Description("编辑器设置")]
	public class InScreenEditSetting : ISingletonSetting<InScreenEditSetting>
	{
		[Description("创建轨道时是否使轨道的初始结束时间为乐曲结束时间")]
		public NotifiableProperty<bool> IsInitialTrackLengthToEnd { get; set; } = new(true);

		[Description("若上一项为false，则轨道的初始长度 | 单位：毫秒")]
		public NotifiableProperty<T3Time> InitialTrackLength { get; set; } = new(3000);

		[Description("当鼠标位置与最近的横向格线时间差小于该值时，将吸附到该格线上 | 单位：毫秒")]
		public NotifiableProperty<T3Time> TimeSnapDistance { get; set; } = new(1000);

		[Description("当鼠标位置与最近的纵向格线距离差小于该值时，将吸附到该格线上")]
		public NotifiableProperty<float> WidthSnapDistance { get; set; } = new(0.5f);

		[Description("单击 W 或 S 时 Note 移动的时间长度 | 单位：毫秒")]
		public NotifiableProperty<T3Time> NoteNudgeDistance { get; set; } = new(10);

		[Description("四分音符（拍）的横向格线颜色 | 四个值代表颜色的RGBA，范围(0, 1)，请勿省略括号或Alpha值")]
		public NotifiableProperty<Color> BeatColor { get; set; } = new(new(0.6f, 0, 0, 0.9f));

		[Description("八分音符的横向格线颜色")]
		public NotifiableProperty<Color> QuaverColor { get; set; } = new(new(0.7f, 0.6f, 0, 0.9f));

		[Description("十六分音符的横向格线颜色")]
		public NotifiableProperty<Color> SemiQuaverColor { get; set; } = new(new(0.7f, 0.6f, 0, 0.9f));

		[Description("十二分音符的横向格线颜色")]
		public NotifiableProperty<Color> TripletColor { get; set; } = new(new(0.7f, 0.5f, 0.7f, 0.9f));

		[Description("不满足上述情况时的横向格线颜色")]
		public NotifiableProperty<Color> DefaultColor { get; set; } = new(new(0.5f, 0.5f, 0.5f, 0.9f));
	}
}