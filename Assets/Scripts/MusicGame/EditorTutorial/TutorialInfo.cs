#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime.Setting;
using T3Framework.Static.Event;

namespace MusicGame.EditorTutorial
{
	public class TutorialInfo : ISingletonSetting<TutorialInfo>
	{
		public NotifiableProperty<List<string>> CompletedTutorials { get; set; } = new(new());
	}
}