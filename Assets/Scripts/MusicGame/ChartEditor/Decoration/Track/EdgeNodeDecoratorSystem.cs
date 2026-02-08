#nullable enable

using System.Linq;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class EdgeNodeDecoratorSystem : HierarchySystem<EdgeNodeDecoratorSystem>
	{
		// Serializable and Public
		[SerializeField] private string edgeNodeNamePrefix = default!;
		[SerializeField] private string directNodeNamePrefix = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Edge
			new ViewPoolPluginRegistrar<EdgePMLComponent, EdgeNodeComponent>(edgeDecoratorPool, edgeViewPool,
				moveList => edgeDataset[moveList]
					.Select(c => (c, $"{edgeNodeNamePrefix}{c.Locator.Time.Milli}"))),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (edgeDataset[component] is not { } moveList ||
					    edgeDecoratorPool[moveList] is not { } decorator) return;
					if (edgeViewPool.Add(component))
					{
						decorator.AddPlugin($"{edgeNodeNamePrefix}{component.Locator.Time.Milli}",
							edgeViewPool[component]!);
					}
				}),
			new DatasetRegistrar<EdgeNodeComponent>(edgeDataset,
				DatasetRegistrar<EdgeNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (edgeDataset[component] is not { } moveList ||
					    edgeDecoratorPool[moveList] is not { } decorator) return;
					var handler = edgeViewPool[component];
					if (edgeViewPool.Remove(component))
						decorator.RemovePlugin(handler!, edgeDecoratorPool.DefaultTransform);
				}),
			// Direct
			new ViewPoolPluginRegistrar<DirectPMLComponent, DirectNodeComponent>(directDecoratorPool, directViewPool,
				moveList => directDataset[moveList]
					.Select(c => (c, $"{directNodeNamePrefix}{c.Locator.Time.Milli}"))),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataAdded,
				component =>
				{
					if (directDataset[component] is not { } moveList ||
					    directDecoratorPool[moveList] is not { } decorator) return;
					if (directViewPool.Add(component))
					{
						decorator.AddPlugin($"{directNodeNamePrefix}{component.Locator.Time.Milli}",
							directViewPool[component]!);
					}
				}),
			new DatasetRegistrar<DirectNodeComponent>(directDataset,
				DatasetRegistrar<DirectNodeComponent>.RegisterTarget.DataRemoved,
				component =>
				{
					if (directDataset[component] is not { } moveList ||
					    directDecoratorPool[moveList] is not { } decorator) return;
					var handler = directViewPool[component];
					if (directViewPool.Remove(component))
						decorator.RemovePlugin(handler!, directDecoratorPool.DefaultTransform);
				})
		};

		// Private
		private EdgeNodeDataset edgeDataset = default!;
		private IViewPool<EdgePMLComponent> edgeDecoratorPool = default!;
		private IViewPool<EdgeNodeComponent> edgeViewPool = default!;
		private DirectNodeDataset directDataset = default!;
		private IViewPool<DirectPMLComponent> directDecoratorPool = default!;
		private IViewPool<DirectNodeComponent> directViewPool = default!;

		// Constructor
		[Inject]
		private void Construct(
			EdgeNodeDataset edgeDataset,
			IViewPool<EdgePMLComponent> edgeDecoratorPool,
			IViewPool<EdgeNodeComponent> edgeViewPool,
			DirectNodeDataset directDataset,
			IViewPool<DirectPMLComponent> directDecoratorPool,
			IViewPool<DirectNodeComponent> directViewPool)
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