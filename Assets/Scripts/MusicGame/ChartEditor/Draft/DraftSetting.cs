#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftSetting : ISingletonSetting<DraftSetting>
	{
		[Description("DefaultDraftNoteWidth")]
		public NotifiableProperty<float> DefaultDraftNoteWidth { get; set; } = new(1);

		[Description("DraftNoteWidthIncrement")]
		public NotifiableProperty<float> DraftNoteWidthIncrement { get; set; } = new(0.1f);

		[Description("DraftNodePositionNudgeDistance")]
		public NotifiableProperty<float> DraftNotePositionNudgeDistance { get; set; } = new(0.1f);

		[Description("DraftNoteOpacityInNormalMode")]
		public NotifiableProperty<float> DraftNoteOpacityInNormalMode { get; set; } = new(0.1f);
	}
}