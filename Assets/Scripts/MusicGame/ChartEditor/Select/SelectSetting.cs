using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.Select
{
	public class SelectSetting : ISingletonSetting<SelectSetting>
	{
		public Color TrackSelectedColor { get; set; } = new(0.01f, 0f, 0.5f, 1f);
	}
}