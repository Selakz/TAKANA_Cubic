using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackLineSetting : ISingletonSetting<TrackLineSetting>
	{
		[Description("默认曲线族的缓动ID | 范围 [0, 9]")]
		public int DefaultEaseId { get; set; } = 1;

		[Description("不可编辑时轨道线的宽度")]
		public float UneditableLineWidth { get; set; } = 0.03f;

		[Description("可编辑时轨道线的宽度")]
		public float EditableLineWidth { get; set; } = 0.05f;

		[Description("单击 A 或 D 时结点移动的距离")]
		public float NodePositionNudgeDistance { get; set; } = 0.1f;

		[Description("单击 W 或 S 时结点移动的时间长度 | 单位：毫秒")]
		public T3Time NodeTimeNudgeDistance { get; set; } = 10;

		[Description("在编辑面板点击添加结点时，新结点与原结点的时间间隔 | 单位：毫秒")]
		public T3Time AddNodeTimeDistance { get; set; } = 100;
	}
}