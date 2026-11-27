#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Track
{
	public class EditTrackNodeDecoratorSystem : HierarchySystem<EditTrackNodeDecoratorSystem>
	{
		// Serializable and Public
		[SerializeField] private string nodeNamePrefix = "TrackNode";

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler =>
			{
				var track = decoratorPool[handler]!;
				var collapsable = handler.Script<EditTrackContent>().MovementContent.Collapsable;
				return new UnionRegistrar(
					new CustomRegistrar(
						() => collapsable.IsCollapsed.Value = false,
						() =>
						{
							ReleaseTrackNodes(track);
							collapsable.IsCollapsed.Value = false;
						}),
					new PropertyRegistrar<bool>(collapsable.IsCollapsed, isExpanded =>
					{
						if (isExpanded) ExpandTrack(track, handler);
					}));
			}, true),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataAdded,
				node =>
				{
					if (!expandedTracks.Contains(node.Track)) return;
					if (decoratorPool[node.Track] is not { } decorator) return;
					if (nodePool.Add(node))
					{
						AttachNode(node, decorator, nodePool[node]!);
					}
				}),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved,
				node => nodePool.Remove(node)),
			new DatasetRegistrar<TrackNodeComponent>(nodePool,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataUpdated,
				node =>
				{
					if (nodePool[node] is { } handler &&
					    decoratorPool[node.Track] is { } decorator &&
					    handler.Parent != decorator)
					{
						AttachNode(node, decorator, handler);
					}
				}),
			new ViewPoolRegistrar<TrackNodeComponent>(nodePool,
				ViewPoolRegistrar<TrackNodeComponent>.RegisterTarget.Release,
				handler => handler.Parent?.RemovePlugin(handler, nodePool.DefaultTransform))
		};

		// Private
		[Inject] [Key("track-decoration")] private readonly IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private readonly TrackNodeDataset nodeDataset = default!;
		[Inject] private readonly IViewPool<TrackNodeComponent> nodePool = default!;

		private readonly HashSet<ChartComponent> expandedTracks = new();

		// Defined Functions
		private void ExpandTrack(ChartComponent track, PrefabHandler decorator)
		{
			if (!expandedTracks.Add(track)) return;
			foreach (var node in nodeDataset[track].OrderBy(node => node.Time.Value))
			{
				if (nodePool.Add(node))
				{
					AttachNode(node, decorator, nodePool[node]!);
				}
			}
		}

		private void ReleaseTrackNodes(ChartComponent track)
		{
			expandedTracks.Remove(track);
			List<TrackNodeComponent> toRemove = new();
			foreach (var node in nodePool)
			{
				if (node.Track == track) toRemove.Add(node);
			}

			foreach (var node in toRemove)
			{
				nodePool.Remove(node);
			}
		}

		private void AttachNode(TrackNodeComponent node, PrefabHandler decorator, PrefabHandler handler)
		{
			decorator.AddPlugin($"{nodeNamePrefix}_{(node.IsFirst ? 1 : 2)}_{node.GetHashCode()}", handler);
			var movement = decorator.Script<EditTrackContent>().MovementContent;
			var targetList = node.IsFirst ? movement.MoveList1 : movement.MoveList2;
			handler.transform.SetParent(targetList.ListItemRoot, false);
		}
	}
}
