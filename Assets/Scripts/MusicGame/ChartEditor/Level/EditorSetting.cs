#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;

namespace MusicGame.ChartEditor.Level
{
	[Description("Header")]
	public class EditorSetting : ISingletonSetting<EditorSetting>
	{
		[Description("ScrollSensitivity")]
		[MinValue(10)]
		[MaxValue(-10)]
		public NotifiableProperty<int> ScrollSensitivity { get; set; } = new(1);

		[Description("ShouldForcePause")]
		public NotifiableProperty<bool> ShouldForcePause { get; set; } = new(true);

		[Description("AutoSaveInterval")]
		[MinValue(30000)]
		public NotifiableProperty<T3Time> AutoSaveInterval { get; set; } = new(180000);

		[Description("SaveIndented")]
		public NotifiableProperty<bool> SaveIndented { get; set; } = new(true);

		[Description("WindowWidth")]
		[MinValue(720)]
		[MaxValue(3840)]
		public NotifiableProperty<int> WindowWidth { get; set; } = new(1920);

		[Description("WindowWidthChoices")]
		public NotifiableProperty<List<int>> WindowWidthChoices { get; set; } = new(new()
		{
			2560,
			1920,
			1600
		});

		[Description("MouseDragThreshold")]
		[MinValue(0)]
		public NotifiableProperty<int> MouseDragThreshold { get; set; } = new(10);

		[Description("RecentProjectBufferCount")]
		[MinValue(0)]
		[MaxValue(100)]
		public NotifiableProperty<int> RecentProjectBufferCount { get; set; } = new(10);

		[Description("RecentProjects")]
		public NotifiableProperty<List<string>> RecentProjects { get; set; } = new(new());

		[Description("LastProjectDirectory")]
		public NotifiableProperty<string> LastProjectDirectory { get; set; } = new(string.Empty);

		[Description("LastCharterId")]
		public NotifiableProperty<string> LastCharterId { get; set; } = new(string.Empty);
	}
}