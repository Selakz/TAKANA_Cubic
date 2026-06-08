#nullable enable

using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftContainer
	{
		public NotifiableProperty<bool> IsInDraftMode { get; } = new(false);
	}

	public class DraftNoteInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<DraftContainer>(Lifetime.Singleton);
		}
	}
}