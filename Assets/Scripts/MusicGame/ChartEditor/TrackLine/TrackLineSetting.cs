using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.TrackLine
{
	public class TrackLineSetting : ISingletonSetting<TrackLineSetting>
	{
		[Description("Ĭ��������Ļ���ID | ��Χ [0, 9]")]
		public int DefaultEaseId { get; set; } = 1;

		[Description("���ɱ༭ʱ����ߵĿ��")]
		public float UneditableLineWidth { get; set; } = 0.03f;

		[Description("�ɱ༭ʱ����ߵĿ��")]
		public float EditableLineWidth { get; set; } = 0.05f;

		[Description("���� A �� D ʱ����ƶ��ľ���")]
		public float NodePositionNudgeDistance { get; set; } = 0.1f;

		[Description("���� W �� S ʱ����ƶ���ʱ�䳤�� | ��λ������")]
		public T3Time NodeTimeNudgeDistance { get; set; } = 10;

		[Description("�ڱ༭�������ӽ��ʱ���½����ԭ����ʱ���� | ��λ������")]
		public T3Time AddNodeTimeDistance { get; set; } = 100;
	}
}