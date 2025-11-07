#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.Utility.AutoUpdate
{
	[Description("自动更新设置")]
	public class AutoUpdateSetting : ISingletonSetting<AutoUpdateSetting>
	{
		[Description("是否在启动时自动检查更新 | 建议开启")]
		public NotifiableProperty<bool> CheckUpdateOnStartup { get; set; } = new(true);

		[Description("是否静默下载")]
		public NotifiableProperty<bool> DownloadSilently { get; set; } = new(false);
	}
}