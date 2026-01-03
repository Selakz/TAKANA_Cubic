#nullable enable

using System.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class EdgeNodeDecoratorSystem : T3System
	{
		private readonly string nodeNamePrefix;
		private readonly EdgeNodeDataset dataset;
		private readonly IViewPool<EdgePMLComponent> decoratorPool;
		private readonly IViewPool<EdgeNodeComponent> viewPool;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<EdgePMLComponent, EdgeNodeComponent>(decoratorPool, viewPool,
				moveList => dataset[moveList]
					.Select(c => (c, $"{nodeNamePrefix}{c.Locator.Time.Milli}"))),
			new DatasetRegistrar<EdgeNodeComponent>(dataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (dataset[component] is not { } moveList || decoratorPool[moveList] is not { } decorator) return;
					if (viewPool.Add(component))
					{
						decorator.AddPlugin($"{nodeNamePrefix}{component.Locator.Time.Milli}", viewPool[component]!);
					}
				}),
			new DatasetRegistrar<EdgeNodeComponent>(dataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (dataset[component] is not { } moveList || decoratorPool[moveList] is not { } decorator) return;
					var handler = viewPool[component];
					if (viewPool.Remove(component)) decorator.RemovePlugin(handler!, decoratorPool.DefaultTransform);
				})
		};

		public EdgeNodeDecoratorSystem(
			string nodeNamePrefix,
			EdgeNodeDataset dataset,
			IViewPool<EdgePMLComponent> decoratorPool,
			IViewPool<EdgeNodeComponent> viewPool) : base(true)
		{
			this.nodeNamePrefix = nodeNamePrefix;
			this.dataset = dataset;
			this.decoratorPool = decoratorPool;
			this.viewPool = viewPool;
		}
	}
}