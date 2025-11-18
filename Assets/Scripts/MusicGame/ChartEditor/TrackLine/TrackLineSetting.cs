using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;

namespace MusicGame.ChartEditor.TrackLine
{
	[Description("轨道线设置")]
	public class TrackLineSetting : ISingletonSetting<TrackLineSetting>
	{
		[Description("默认曲线族的缓动ID | 范围 [0, 9]")]
		[MinValue(0)]
		[MaxValue(9)]
		public NotifiableProperty<int> DefaultEaseId { get; set; } = new(1);

		[Description("单击 A 或 D 时结点移动的距离")]
		public NotifiableProperty<float> NodePositionNudgeDistance { get; set; } = new(0.1f);

		[Description("单击 W 或 S 时结点移动的时间长度 | 单位：毫秒")]
		[MinValue(-10000)]
		[MaxValue(10000)]
		public NotifiableProperty<T3Time> NodeTimeNudgeDistance { get; set; } = new(10);

		[Description("不可编辑时轨道线的宽度")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> UneditableLineWidth { get; set; } = new(0.03f);

		[Description("可编辑时轨道线的宽度")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> EditableLineWidth { get; set; } = new(0.05f);

		[Description("轨道线渲染精度，若轨道编辑卡顿可适当增加该值")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> ViewLinePrecision { get; set; } = new(0.01f);

		[Description("轨道线碰撞箱渲染精度，若轨道编辑卡顿可适当增加该值")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> LogicLinePrecision { get; set; } = new(0.2f);

		[Description("轨道线的最大分段数，若轨道编辑卡顿可适当减少该值")]
		[MinValue(1)]
		public NotifiableProperty<int> MaxSegment { get; set; } = new(100);

		[HideInGame]
		[Description("轨道线碰撞箱渲染的宽度")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> LogicLineWidth { get; set; } = new(0.3f);

		[Description("在编辑面板点击添加结点时，新结点与原结点的时间间隔 | 单位：毫秒")]
		[MinValue(1)]
		public NotifiableProperty<T3Time> AddNodeTimeDistance { get; set; } = new(100);

		[Description("下落速度变化时，轨道线将会等待该时间后再重新渲染，以减少卡顿 | 单位：毫秒")]
		[MinValue(0)]
		public NotifiableProperty<T3Time> SpeedChangeRerenderDelay { get; set; } = new(1500);
	}
}