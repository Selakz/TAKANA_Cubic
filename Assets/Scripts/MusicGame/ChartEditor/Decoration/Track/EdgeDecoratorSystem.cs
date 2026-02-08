#nullable enable

using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class EdgeDecoratorSystem : HierarchySystem<EdgeDecoratorSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent, EdgeComponent>(decoratorPool, edgeViewPool,
				track =>
				{
					return track.Model is ITrack { Movement: TrackEdgeMovement }
						? edgeDataset[track].Select(m => (m, "EdgeDecorator"))
						: Enumerable.Empty<(EdgeComponent, string)>();
				}),
			new DatasetRegistrar<EdgeComponent>(edgeDataset,
				DatasetRegistrar<EdgeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					var track = component.Locator.Track;
					decoratorPool[track]?.RemovePlugin("EdgeDecorator", edgeViewPool.DefaultTransform);
					edgeViewPool.Remove(component);
				}),
			new ViewPoolPluginRegistrar<ChartComponent, DirectComponent>(decoratorPool, directViewPool,
				track =>
				{
					return track.Model is ITrack { Movement: TrackDirectMovement }
						? directDataset[track].Select(m => (m, "DirectDecorator"))
						: Enumerable.Empty<(DirectComponent, string)>();
				}),
			new DatasetRegistrar<DirectComponent>(directDataset,
				DatasetRegistrar<DirectComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					var track = component.Locator.Track;
					decoratorPool[track]?.RemovePlugin("DirectDecorator", edgeViewPool.DefaultTransform);
					directViewPool.Remove(component);
				})
		};

		// Private
		private IViewPool<ChartComponent> decoratorPool = default!;
		private EdgeDataset edgeDataset = default!;
		private IViewPool<EdgeComponent> edgeViewPool = default!;
		private DirectDataset directDataset = default!;
		private IViewPool<DirectComponent> directViewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			[Key("track-decoration")] IViewPool<ChartComponent> decoratorPool,
			EdgeDataset edgeDataset,
			IViewPool<EdgeComponent> edgeViewPool,
			DirectDataset directDataset,
			IViewPool<DirectComponent> directViewPool)
		{
			this.decoratorPool = decoratorPool;
			this.edgeDataset = edgeDataset;
			this.edgeViewPool = edgeViewPool;
			this.directDataset = directDataset;
			this.directViewPool = directViewPool;
		}
	}
}