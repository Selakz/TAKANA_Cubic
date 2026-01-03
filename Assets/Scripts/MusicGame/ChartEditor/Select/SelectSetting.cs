using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	[Description("Header")]
	public class SelectSetting : ISingletonSetting<SelectSetting>
	{
		[Description("TrackSelectedColor")]
		public NotifiableProperty<Color> TrackSelectedColor { get; set; } = new(new(0.01f, 0f, 0.5f, 1f));
	}
}