#nullable enable

using System.Linq;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackNodeDecoratorSystem : HierarchySystem<TrackNodeDecoratorSystem>
	{
		// Serializable and Public
		[SerializeField] private string nodeNamePrefix = "TrackNode";

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolPluginRegistrar<ChartComponent, TrackNodeComponent>(decoratorPool, nodePool,
				track => nodeDataset[track].Select(node => (node, GetPluginId(node)))),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataAdded,
				node =>
				{
					if (decoratorPool[node.Track] is not { } decorator) return;
					if (nodePool.Add(node))
					{
						decorator.AddPlugin(GetPluginId(node), nodePool[node]!);
					}
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved,
				node =>
				{
					if (nodePool[node] is not { } handler) return;
					decoratorPool[node.Track]?.RemovePlugin(handler, nodePool.DefaultTransform);
					nodePool.Remove(node);
				}),
			new ViewPoolRegistrar<TrackNodeComponent>(nodePool,
				ViewPoolRegistrar<TrackNodeComponent>.RegisterTarget.Release,
				handler => handler.Parent?.RemovePlugin(handler, nodePool.DefaultTransform)),
			new DatasetRegistrar<TrackNodeComponent>(nodePool,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated,
				node =>
				{
					if (nodePool[node] is { } handler &&
					    decoratorPool[node.Track] is { } decorator &&
					    handler.Parent != decorator)
					{
						decorator.AddPlugin(GetPluginId(node), handler);
					}
				})
		};

		// Private
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private readonly TrackNodeDataset nodeDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;

		// Defined Functions
		public static bool TryParseIsFirst(string pluginId, out bool isFirst)
		{
			var lastUnderscore = pluginId.LastIndexOf('_');
			if (lastUnderscore >= 2 && pluginId[lastUnderscore - 2] == '_')
			{
				var side = pluginId[lastUnderscore - 1];
				if (side is '1' or '2')
				{
					isFirst = side == '1';
					return true;
				}
			}

			isFirst = false;
			return false;
		}

		private string GetPluginId(TrackNodeComponent node) =>
			$"{nodeNamePrefix}_{(node.IsFirst ? 1 : 2)}_{node.GetHashCode()}";
	}
}
