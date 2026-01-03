using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	[Description("Header")]
	public class TrackLineSetting : ISingletonSetting<TrackLineSetting>
	{
		[Description("DefaultEaseId")]
		[MinValue(0)]
		[MaxValue(9)]
		public NotifiableProperty<int> DefaultEaseId { get; set; } = new(1);

		[Description("AllowMultipleEdit")]
		public NotifiableProperty<bool> AllowMultipleEdit { get; set; } = new(false);

		[Description("NodePositionNudgeDistance")]
		public NotifiableProperty<float> NodePositionNudgeDistance { get; set; } = new(0.1f);

		[Description("NodeTimeNudgeDistance")]
		[MinValue(-10000)]
		[MaxValue(10000)]
		public NotifiableProperty<T3Time> NodeTimeNudgeDistance { get; set; } = new(10);

		[Description("UneditableLineWidth")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> UneditableLineWidth { get; set; } = new(0.03f);

		[Description("EditableLineWidth")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> EditableLineWidth { get; set; } = new(0.05f);

		[Description("LeftSideNodeColor")]
		public NotifiableProperty<Color> LeftSideNodeColor { get; set; } = new(new Color(0.9f, 1f, 1f));

		[Description("RightSideNodeColor")]
		public NotifiableProperty<Color> RightSideNodeColor { get; set; } = new(new Color(1f, 0.9f, 0.9f));

		[Description("SelectedLeftColor")]
		public NotifiableProperty<Color> SelectedLeftColor { get; set; } = new(new Color(0.38f, 0.60f, 0.82f));

		[Description("SelectedRightColor")]
		public NotifiableProperty<Color> SelectedRightColor { get; set; } = new(new Color(0.60f, 0.38f, 0.82f));

		[Description("LogicLinePrecision")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> LogicLinePrecision { get; set; } = new(0.2f);

		[Description("MaxSegment")]
		[MinValue(1)]
		public NotifiableProperty<int> MaxSegment { get; set; } = new(100);

		[HideInGame]
		[Description("LogicLineWidth")]
		[MinValue(0.001f)]
		public NotifiableProperty<float> LogicLineWidth { get; set; } = new(0.3f);

		[Description("AddNodeTimeDistance")]
		[MinValue(1)]
		public NotifiableProperty<T3Time> AddNodeTimeDistance { get; set; } = new(100);
	}
}