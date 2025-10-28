using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	[Description("�༭UI����")]
	public class AutoScoreSetting : ISingletonSetting<AutoScoreSetting>
	{
		[Description("��Note�Ĳ�͸���� | ��Χ [0, 1]")]
		public NotifiableProperty<float> DummyNoteOpacity { get; set; } = new(0.5f);
	}
}