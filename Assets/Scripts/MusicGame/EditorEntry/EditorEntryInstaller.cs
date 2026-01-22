#nullable enable

using MusicGame.ChartEditor.Level;
using MusicGame.LevelSelect;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.EditorEntry
{
	public class EditorEntryInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<IDataset<LevelComponent<EditorPreference>>, ListDataset<LevelComponent<EditorPreference>>>
				(Lifetime.Singleton).AsSelf();
		}
	}
}