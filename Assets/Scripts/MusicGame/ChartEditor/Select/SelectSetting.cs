using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	[Description("点选设置")]
	public class SelectSetting : ISingletonSetting<SelectSetting>
	{
		[Description("轨道被选中时的颜色")]
		public NotifiableProperty<Color> TrackSelectedColor { get; set; } = new(new(0.01f, 0f, 0.5f, 1f));
	}
}