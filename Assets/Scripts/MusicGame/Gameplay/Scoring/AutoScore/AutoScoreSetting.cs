using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;
using T3Framework.Static.Setting;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	[Description("Header")]
	public class AutoScoreSetting : ISingletonSetting<AutoScoreSetting>
	{
		[Description("DummyNoteOpacity")]
		[MinValue(0)]
		[MaxValue(1)]
		public NotifiableProperty<float> DummyNoteOpacity { get; set; } = new(0.5f);
	}
}