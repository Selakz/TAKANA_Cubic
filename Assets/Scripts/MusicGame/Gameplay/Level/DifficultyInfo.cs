#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Localization;
using T3Framework.Runtime.Setting;

namespace MusicGame.Gameplay.Level
{
	public class DifficultyInfo : ISetting<DifficultyInfo>
	{
		[Description("չʾ���Ѷȵȼ�����")]
		public string LevelDisplay { get; set; } = "00";

		[Description("չʾ����ʦ��Ϣ")]
		public I18NString Charter { get; set; } = new();
	}
}