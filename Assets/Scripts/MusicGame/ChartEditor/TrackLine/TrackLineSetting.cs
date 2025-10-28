using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.TrackLine
{
	[Description("轨道线设置")]
	public class TrackLineSetting : ISingletonSetting<TrackLineSetting>
	{
		[Description("默认曲线族的缓动ID | 范围 [0, 9]")]
		public NotifiableProperty<int> DefaultEaseId { get; set; } = new(1);

		[Description("不可编辑时轨道线的宽度")]
		public NotifiableProperty<float> UneditableLineWidth { get; set; } = new(0.03f);

		[Description("可编辑时轨道线的宽度")]
		public NotifiableProperty<float> EditableLineWidth { get; set; } = new(0.05f);

		[Description("单击 A 或 D 时结点移动的距离")]
		public NotifiableProperty<float> NodePositionNudgeDistance { get; set; } = new(0.1f);

		[Description("单击 W 或 S 时结点移动的时间长度 | 单位：毫秒")]
		public NotifiableProperty<T3Time> NodeTimeNudgeDistance { get; set; } = new(10);

		[Description("在编辑面板点击添加结点时，新结点与原结点的时间间隔 | 单位：毫秒")]
		public NotifiableProperty<T3Time> AddNodeTimeDistance { get; set; } = new(100);
	}
}