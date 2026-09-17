#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.Utility
{
	[Description("Header")]
	public class ExperimentalSetting : ISingletonSetting<ExperimentalSetting>
	{
		[Description("UseExperimentalAudioPlayer")]
		public NotifiableProperty<bool> UseExperimentalAudioPlayer { get; set; } = new(false);
	}
}