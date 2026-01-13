#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace App.AutoUpdate
{
	[Description("Header")]
	public class AutoUpdateSetting : ISingletonSetting<AutoUpdateSetting>
	{
		[Description("CheckUpdateOnStartup")]
		public NotifiableProperty<bool> CheckUpdateOnStartup { get; set; } = new(true);

		[Description("DownloadSilently")]
		public NotifiableProperty<bool> DownloadSilently { get; set; } = new(false);
	}
}