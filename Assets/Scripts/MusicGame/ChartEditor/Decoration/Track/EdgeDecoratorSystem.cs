#nullable enable

using System.Linq;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class EdgeDecoratorSystem : T3System
	{
		private readonly EdgeDataset dataset;
		private readonly IViewPool<ChartComponent> decoratorPool;
		private readonly IViewPool<EdgeComponent> viewPool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent, EdgeComponent>(decoratorPool, viewPool,
				track => dataset[track].Select(m => (m, "EdgeDecorator"))),
			new DatasetRegistrar<EdgeComponent>(dataset,
				DatasetRegistrar<EdgeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					var track = component.Locator.Track;
					decoratorPool[track]?.RemovePlugin("EdgeDecorator", viewPool.DefaultTransform);
					viewPool.Remove(component);
				})
		};

		public EdgeDecoratorSystem(
			EdgeDataset dataset,
			[Key("track-decoration")] IViewPool<ChartComponent> decoratorPool,
			IViewPool<EdgeComponent> viewPool) : base(true)
		{
			this.dataset = dataset;
			this.decoratorPool = decoratorPool;
			this.viewPool = viewPool;
		}
	}
}