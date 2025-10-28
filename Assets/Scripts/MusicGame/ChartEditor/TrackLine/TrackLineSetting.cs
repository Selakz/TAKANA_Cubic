using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.TrackLine
{
	[Description("���������")]
	public class TrackLineSetting : ISingletonSetting<TrackLineSetting>
	{
		[Description("Ĭ��������Ļ���ID | ��Χ [0, 9]")]
		public NotifiableProperty<int> DefaultEaseId { get; set; } = new(1);

		[Description("���ɱ༭ʱ����ߵĿ��")]
		public NotifiableProperty<float> UneditableLineWidth { get; set; } = new(0.03f);

		[Description("�ɱ༭ʱ����ߵĿ��")]
		public NotifiableProperty<float> EditableLineWidth { get; set; } = new(0.05f);

		[Description("���� A �� D ʱ����ƶ��ľ���")]
		public NotifiableProperty<float> NodePositionNudgeDistance { get; set; } = new(0.1f);

		[Description("���� W �� S ʱ����ƶ���ʱ�䳤�� | ��λ������")]
		public NotifiableProperty<T3Time> NodeTimeNudgeDistance { get; set; } = new(10);

		[Description("�ڱ༭�������ӽ��ʱ���½����ԭ����ʱ���� | ��λ������")]
		public NotifiableProperty<T3Time> AddNodeTimeDistance { get; set; } = new(100);
	}
}