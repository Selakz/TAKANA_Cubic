#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Decoration.Note
{
	public class NoteDecorationInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			var noteDataset = new HashDataset<ChartComponent>();
			builder.RegisterInstance<IDataset<ChartComponent>>(noteDataset)
				.Keyed("note-decoration");
			builder.RegisterEntryPoint<NoteSourceSystem>();
		}
	}
}