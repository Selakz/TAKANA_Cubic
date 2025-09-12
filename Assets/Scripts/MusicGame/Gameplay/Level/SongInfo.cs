#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using T3Framework.Runtime.Localization;
using T3Framework.Runtime.Setting;

namespace MusicGame.Gameplay.Level
{
	/// <summary> All about a song </summary>
	public class SongInfo : ISetting<SongInfo>
	{
		[Description("������Ψһ��ʶ��")]
		public string Id { get; set; } = string.Empty;

		[Description("չʾ������")]
		public I18NString Title { get; set; } = new();

		[Description("չʾ����ʦ")]
		public I18NString Composer { get; set; } = new();

		[Description("չʾ�Ļ�ʦ")]
		public I18NString Illustrator { get; set; } = new();

		[Description("չʾ��BPM��Ϣ")]
		public string BpmDisplay { get; set; } = string.Empty;

		[Description("�Ը���������")]
		public I18NString Description { get; set; } = new();

		[Description("���Ѷ���Ϣ")]
		public Dictionary<int, DifficultyInfo> Difficulties { get; set; } = new();
	}
}