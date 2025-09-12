using System.ComponentModel;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.Scoring.AutoScore
{
	public class AutoScoreSetting : ISingletonSetting<AutoScoreSetting>
	{
		[Description("��Note�Ĳ�͸���� | ��Χ [0, 1]")]
		public float DummyNoteOpacity { get; set; } = 0.5f;
	}
}