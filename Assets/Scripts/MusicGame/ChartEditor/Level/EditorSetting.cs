#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Level
{
	[Description("Ӧ������")]
	public class EditorSetting : ISingletonSetting<EditorSetting>
	{
		[Description("�����������С�ٷֱȣ������� | ��Χ [0, 100]")]
		public NotifiableProperty<int> HitSoundVolumePercent { get; set; } = new(50);

		[Description("�����������ȣ���ÿ�ι����ƶ��ĸ�����������������")]
		public NotifiableProperty<int> ScrollSensitivity { get; set; } = new(1);

		[Description("�Զ������ʱ���� | ��λ: ����")]
		public NotifiableProperty<T3Time> AutoSaveInterval { get; set; } = new(180000);

		[Description("���ڿ�ȣ��߶Ƚ��ڱ���16 : 9���ڱ�����ǰ�����Զ�����")]
		public NotifiableProperty<int> WindowWidth { get; set; } = new(1920);

		[Description("�ڱ༭�������ṩ�ķֱ��ʵ���ѡ�ͬ����Ϊ���ڿ��")]
		public NotifiableProperty<List<int>> WindowWidthChoices { get; set; } = new(new()
		{
			2560,
			1920,
			1600
		});
	}
}