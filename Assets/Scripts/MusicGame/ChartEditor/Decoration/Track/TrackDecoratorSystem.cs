#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackDecoratorSystem : HierarchySystem<TrackDecoratorSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<ChartComponent>(trackDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded,
				component => decoratorPool.Add(component)),
			new DatasetRegistrar<ChartComponent>(trackDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved,
				component => decoratorPool.Remove(component))
		};

		// Private
		private IDataset<ChartComponent> trackDataset = default!;
		private IViewPool<ChartComponent> decoratorPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			[Key("track-decoration")] IDataset<ChartComponent> trackDataset,
			[Key("track-decoration")] IViewPool<ChartComponent> decoratorPool)
		{
			this.trackDataset = trackDataset;
			this.decoratorPool = decoratorPool;
		}
	}
}