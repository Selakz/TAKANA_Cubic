#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecorationInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			// Track Decorator
			var trackDataset = new HashDataset<ChartComponent>();
			builder.RegisterInstance<IDataset<ChartComponent>>(trackDataset)
				.Keyed("track-decoration");

			// Track Node
			var trackNodeDataset = new TrackNodeDataset();
			builder.RegisterInstance(trackNodeDataset)
				.AsSelf()
				.As<IDataset<TrackNodeComponent>>()
				.As<IReadOnlyDataset<TrackNodeComponent>>();
			builder.Register<TrackNodeSelectDataset>(Lifetime.Singleton);
		}
	}
}
