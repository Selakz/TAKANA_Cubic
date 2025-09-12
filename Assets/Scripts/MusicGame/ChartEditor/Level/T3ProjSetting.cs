using System.ComponentModel;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.Level
{
	[SettingFileName(".t3proj")]
	public class T3ProjSetting : ISetting<T3ProjSetting>
	{
		[Description("��Դ�ļ����� | �����ļ���Ҫ�����ļ���׺��")]
		public string MusicFileName { get; set; } = "music.mp3";

		[Description("�����ļ�����")]
		public string CoverFileName { get; set; } = "cover.jpg";

		[Description("������Ϣ�ļ�����")]
		public string SongInfoFileName { get; set; } = "songinfo.yaml";

		[Description("ƫ�������ļ����ƣ�����������༭���и�����������༭����һЩ����")]
		public string PreferenceFileName { get; set; } = "preference.yaml";

		[Description("���Ѷ������ļ����� | �����ļ�����Ҫ�����ļ���׺")]
		public string NormalChartFileName { get; set; } = "normal";

		public string HardChartFileName { get; set; } = "hard";

		public string MasterChartFileName { get; set; } = "master";

		public string InsanityChartFileName { get; set; } = "insanity";

		public string RavageChartFileName { get; set; } = "ravage";

		public string GetChartFileName(int difficulty)
		{
			return difficulty switch
			{
				1 => NormalChartFileName,
				2 => HardChartFileName,
				3 => MasterChartFileName,
				4 => InsanityChartFileName,
				5 => RavageChartFileName,
				_ => MasterChartFileName,
			};
		}
	}
}