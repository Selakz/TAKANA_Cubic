#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.Level
{
	public class EditorSetting : ISingletonSetting<EditorSetting>
	{
		[Description("�����������С�ٷֱȣ������� | ��Χ [0, 100]")]
		public int HitSoundVolumePercent { get; set; } = 50;

		[Description("�����������ȣ���ÿ�ι����ƶ��ĸ�����������������")]
		public int ScrollSensitivity { get; set; } = 1;

		[Description("�Զ������ʱ���� | ��λ: ����")]
		public T3Time AutoSaveInterval { get; set; } = 180000;

		[Description("���ڿ�ȣ��߶Ƚ��ڱ���16 : 9���ڱ�����ǰ�����Զ�����")]
		public int WindowWidth { get; set; } = 1920;

		[Description("�ڱ༭�������ṩ�ķֱ��ʵ���ѡ�ͬ����Ϊ���ڿ��")]
		public List<int> WindowWidthChoices { get; set; } = new()
		{
			2560,
			1920,
			1600
		};
	}
}