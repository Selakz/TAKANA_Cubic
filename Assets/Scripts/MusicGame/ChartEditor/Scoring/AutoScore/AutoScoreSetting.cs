using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	[Description("编辑UI设置")]
	public class AutoScoreSetting : ISingletonSetting<AutoScoreSetting>
	{
		[Description("假Note的不透明度 | 范围 [0, 1]")]
		public NotifiableProperty<float> DummyNoteOpacity { get; set; } = new(0.5f);
	}
}