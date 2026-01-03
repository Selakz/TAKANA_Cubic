using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	[Description("Header")]
	public class InScreenEditSetting : ISingletonSetting<InScreenEditSetting>
	{
		[Description("IsInitialTrackLengthToEnd")]
		public NotifiableProperty<bool> IsInitialTrackLengthToEnd { get; set; } = new(true);

		[Description("InitialTrackLength")]
		[MinValue(100)]
		public NotifiableProperty<T3Time> InitialTrackLength { get; set; } = new(3000);

		[Description("TimeSnapDistance")]
		[MinValue(0)]
		public NotifiableProperty<T3Time> TimeSnapDistance { get; set; } = new(1000);

		[Description("NoteNudgeDistance")]
		public NotifiableProperty<T3Time> NoteNudgeDistance { get; set; } = new(10);

		[Description("TimeDragThreshold")]
		[MinValue(5)]
		public NotifiableProperty<T3Time> TimeDragThreshold { get; set; } = new(50);

		[Description("ShowTimeIndicator")]
		public NotifiableProperty<bool> ShowTimeIndicator { get; set; } = new(true);

		[Description("ShowPositionIndicator")]
		public NotifiableProperty<bool> ShowPositionIndicator { get; set; } = new(true);

		[Description("BeatColor")]
		public NotifiableProperty<Color> BeatColor { get; set; } = new(new(0.6f, 0, 0, 0.9f));

		[Description("QuaverColor")]
		public NotifiableProperty<Color> QuaverColor { get; set; } = new(new(0.7f, 0.6f, 0, 0.9f));

		[Description("SemiQuaverColor")]
		public NotifiableProperty<Color> SemiQuaverColor { get; set; } = new(new(0.7f, 0.6f, 0, 0.9f));

		[Description("TripletColor")]
		public NotifiableProperty<Color> TripletColor { get; set; } = new(new(0.7f, 0.5f, 0.7f, 0.9f));

		[Description("DefaultColor")]
		public NotifiableProperty<Color> DefaultColor { get; set; } = new(new(0.5f, 0.5f, 0.5f, 0.9f));
	}
}