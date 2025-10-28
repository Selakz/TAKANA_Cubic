#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Level
{
	[Description("应用设置")]
	public class EditorSetting : ISingletonSetting<EditorSetting>
	{
		[Description("打击音音量大小百分比（整数） | 范围 [0, 100]")]
		public NotifiableProperty<int> HitSoundVolumePercent { get; set; } = new(50);

		[Description("鼠标滚动灵敏度（即每次滚动移动的格线数量，负数则反向）")]
		public NotifiableProperty<int> ScrollSensitivity { get; set; } = new(1);

		[Description("自动保存的时间间隔 | 单位: 毫秒")]
		public NotifiableProperty<T3Time> AutoSaveInterval { get; set; } = new(180000);

		[Description("窗口宽度，高度将在保持16 : 9窗口比例的前提下自动计算")]
		public NotifiableProperty<int> WindowWidth { get; set; } = new(1920);

		[Description("在编辑器界面提供的分辨率调整选项，同样均为窗口宽度")]
		public NotifiableProperty<List<int>> WindowWidthChoices { get; set; } = new(new()
		{
			2560,
			1920,
			1600
		});
	}
}