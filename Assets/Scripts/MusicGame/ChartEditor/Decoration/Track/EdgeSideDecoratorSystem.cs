#nullable enable

using System.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class EdgeSideDecoratorSystem : HierarchySystem<EdgeSideDecoratorSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<EdgeComponent, EdgePMLComponent>(edgeDecoratorPool, edgeViewPool,
				movement => edgeDataset[movement]
					.Select(c => (c, $"EdgeSideMovement{(c.Locator.IsLeft ? '1' : '2')}"))),
			new ViewPoolPluginRegistrar<DirectComponent, DirectPMLComponent>(directDecoratorPool, directViewPool,
				movement => directDataset[movement]
					.Select(c => (c, $"DirectSideMovement{(c.Locator.IsPos ? '1' : '2')}"))),
			// TODO: Data Removed (i.e. When a track's edgeMovement's side movement change from ChartPos to others)
		};

		// Private
		private EdgePMLDataset edgeDataset = default!;
		private IViewPool<EdgeComponent> edgeDecoratorPool = default!;
		private IViewPool<EdgePMLComponent> edgeViewPool = default!;
		private DirectPMLDataset directDataset = default!;
		private IViewPool<DirectComponent> directDecoratorPool = default!;
		private IViewPool<DirectPMLComponent> directViewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			EdgePMLDataset edgeDataset,
			IViewPool<EdgeComponent> edgeDecoratorPool,
			IViewPool<EdgePMLComponent> edgeViewPool,
			DirectPMLDataset directDataset,
			IViewPool<DirectComponent> directDecoratorPool,
			IViewPool<DirectPMLComponent> directViewPool)
		{
			this.edgeDataset = edgeDataset;
			this.edgeDecoratorPool = edgeDecoratorPool;
			this.edgeViewPool = edgeViewPool;
			this.directDataset = directDataset;
			this.directDecoratorPool = directDecoratorPool;
			this.directViewPool = directViewPool;
		}
	}
}