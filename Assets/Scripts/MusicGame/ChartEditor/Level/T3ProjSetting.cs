using System.ComponentModel;
using T3Framework.Runtime.Setting;

namespace MusicGame.ChartEditor.Level
{
	[SettingFileName(".t3proj")]
	public class T3ProjSetting : ISetting<T3ProjSetting>
	{
		[Description("音源文件名称 | 以下文件需要附带文件后缀名")]
		public string MusicFileName { get; set; } = "music.mp3";

		[Description("封面文件名称")]
		public string CoverFileName { get; set; } = "cover.jpg";

		[Description("乐曲信息文件名称")]
		public string SongInfoFileName { get; set; } = "songinfo.yaml";

		[Description("偏好设置文件名称，例如在谱面编辑器中该项用来保存编辑器的一些设置")]
		public string PreferenceFileName { get; set; } = "preference.yaml";

		[Description("各难度谱面文件名称 | 以下文件不需要附带文件后缀")]
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