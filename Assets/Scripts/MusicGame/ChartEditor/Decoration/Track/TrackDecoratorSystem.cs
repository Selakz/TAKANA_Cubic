#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecoratorSystem : T3System
	{
		private readonly IDataset<ChartComponent> trackDataset;
		private readonly IViewPool<ChartComponent> decoratorPool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<ChartComponent>(trackDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded,
				component => decoratorPool.Add(component)),
			new DatasetRegistrar<ChartComponent>(trackDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved,
				component => decoratorPool.Remove(component))
		};

		public TrackDecoratorSystem(
			[Key("track-decoration")] IDataset<ChartComponent> trackDataset,
			[Key("track-decoration")] IViewPool<ChartComponent> decoratorPool) : base(true)
		{
			this.trackDataset = trackDataset;
			this.decoratorPool = decoratorPool;
		}
	}
}