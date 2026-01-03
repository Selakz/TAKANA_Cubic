#nullable enable

using System.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class EdgeSideDecoratorSystem : T3System
	{
		private readonly EdgePMLDataset dataset;
		private readonly IViewPool<EdgeComponent> decoratorPool;
		private readonly IViewPool<EdgePMLComponent> viewPool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<EdgeComponent, EdgePMLComponent>(decoratorPool, viewPool,
				movement => dataset[movement]
					.Select(c => (c, $"EdgeSideMovement{(c.Locator.IsLeft ? '1' : '2')}"))),
			// TODO: Data Removed (i.e. When a track's edgeMovement's side movement change from ChartPos to others)
		};

		public EdgeSideDecoratorSystem(
			EdgePMLDataset dataset,
			IViewPool<EdgeComponent> decoratorPool,
			IViewPool<EdgePMLComponent> viewPool) : base(true)
		{
			this.dataset = dataset;
			this.decoratorPool = decoratorPool;
			this.viewPool = viewPool;
		}
	}
}