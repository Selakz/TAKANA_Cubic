#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Movement;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackNodeSourceSystem : HierarchySystem<TrackNodeSourceSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetLifetimeRegistrar<ChartComponent>(trackDataset,
				track => new CustomRegistrar(
					() => AddTrackNodes(track),
					() => RemoveTrackNodes(track)), true),
			new DatasetRegistrar<ChartComponent>(trackDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataUpdated,
				SyncTrackNodes),
			new DatasetRegistrar<TrackNodeComponent>(nodeDataset,
				DatasetRegistrar<TrackNodeComponent>.RegisterTarget.DataRemoved,
				node => nodeSelectDataset.Remove(node))
		};

		// Private
		[Inject, Key("track-decoration")] private readonly IDataset<ChartComponent> trackDataset = default!;
		[Inject] private readonly TrackNodeDataset nodeDataset = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;

		// Defined Functions
		private void AddTrackNodes(ChartComponent track)
		{
			foreach (var entry in GetTargetEntries(track))
			{
				nodeDataset.Add(new TrackNodeComponent(track, entry.Type, entry.Time, entry.Item));
			}

			nodeDataset.NotifyTrackSynced(track);
		}

		private void RemoveTrackNodes(ChartComponent track)
		{
			foreach (var node in nodeDataset[track].ToArray())
			{
				nodeDataset.Remove(node);
			}
		}

		private void SyncTrackNodes(ChartComponent track)
		{
			List<TrackNodeComponent> remainingOld = new(nodeDataset[track]);
			List<NodeEntry> remainingNew = new(GetTargetEntries(track));
			List<(TrackNodeComponent Node, NodeEntry Target)> matched = new();

			// Pass 1: exact (Type, Time, Item reference) match
			MatchPairs((oldNode, target) =>
				oldNode.Type == target.Type &&
				oldNode.Time.Value == target.Time &&
				ReferenceEquals(oldNode.Model, target.Item));

			// Pass 2: same Type and Item reference (preserves identity when time shifts)
			MatchPairs((oldNode, target) =>
				oldNode.Type == target.Type &&
				ReferenceEquals(oldNode.Model, target.Item));

			// Pass 3: same Type and Time (preserves identity when item is replaced in place)
			MatchPairs((oldNode, target) =>
				oldNode.Type == target.Type &&
				oldNode.Time.Value == target.Time);

			foreach (var oldNode in remainingOld)
			{
				nodeDataset.Remove(oldNode);
			}

			foreach (var (node, target) in matched)
			{
				node.Sync(target.Time, target.Item);
			}

			foreach (var entry in remainingNew)
			{
				nodeDataset.Add(new TrackNodeComponent(track, entry.Type, entry.Time, entry.Item));
			}

			nodeDataset.NotifyTrackSynced(track);
			return;

			void MatchPairs(Func<TrackNodeComponent, NodeEntry, bool> predicate)
			{
				for (var i = remainingOld.Count - 1; i >= 0; i--)
				{
					var oldNode = remainingOld[i];
					for (var j = remainingNew.Count - 1; j >= 0; j--)
					{
						var target = remainingNew[j];
						if (!predicate(oldNode, target)) continue;
						matched.Add((oldNode, target));
						remainingOld.RemoveAt(i);
						remainingNew.RemoveAt(j);
						break;
					}
				}
			}
		}

		private static IEnumerable<NodeEntry> GetTargetEntries(ChartComponent track)
		{
			if (track.Model is not ITrack model) yield break;
			switch (model.Movement)
			{
				case TrackEdgeMovement edge:
					if (edge.Movement1 is ChartPosMoveList leftList)
					{
						foreach (var pair in leftList)
						{
							yield return new(TrackNodeType.Left, pair.Key, pair.Value);
						}
					}

					if (edge.Movement2 is ChartPosMoveList rightList)
					{
						foreach (var pair in rightList)
						{
							yield return new(TrackNodeType.Right, pair.Key, pair.Value);
						}
					}

					break;
				case TrackDirectMovement direct:
					if (direct.Movement1 is ChartPosMoveList posList)
					{
						foreach (var pair in posList)
						{
							yield return new(TrackNodeType.Pos, pair.Key, pair.Value);
						}
					}

					if (direct.Movement2 is ChartPosMoveList widthList)
					{
						foreach (var pair in widthList)
						{
							yield return new(TrackNodeType.Width, pair.Key, pair.Value);
						}
					}

					break;
			}
		}

		private readonly struct NodeEntry
		{
			public TrackNodeType Type { get; }
			public T3Time Time { get; }
			public IPositionMoveItem<float> Item { get; }

			public NodeEntry(TrackNodeType type, T3Time time, IPositionMoveItem<float> item)
			{
				Type = type;
				Time = time;
				Item = item;
			}
		}
	}
}