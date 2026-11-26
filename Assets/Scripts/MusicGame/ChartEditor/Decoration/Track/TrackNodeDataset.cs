#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackNodeDataset : HashDataset<TrackNodeComponent>
	{
		private readonly Dictionary<ChartComponent, HashSet<TrackNodeComponent>> trackMap = new();

		public event Action<ChartComponent>? OnTrackSynced;

		public IReadOnlyCollection<TrackNodeComponent> this[ChartComponent track] =>
			trackMap.TryGetValue(track, out var set) ? set : Array.Empty<TrackNodeComponent>();

		public IEnumerable<TrackNodeComponent> GetNodes(ChartComponent track, bool isFirst) =>
			this[track].Where(node => node.IsFirst == isFirst);

		public IEnumerable<TrackNodeComponent> GetNodes(ChartComponent track, TrackNodeType type) =>
			this[track].Where(node => node.Type == type);

		protected override bool AddToDataset(TrackNodeComponent item)
		{
			if (!base.AddToDataset(item)) return false;
			if (!trackMap.TryGetValue(item.Track, out var set))
			{
				set = new();
				trackMap[item.Track] = set;
			}

			set.Add(item);
			return true;
		}

		protected override void RemoveFromDataset(TrackNodeComponent item)
		{
			base.RemoveFromDataset(item);
			if (trackMap.TryGetValue(item.Track, out var set))
			{
				set.Remove(item);
				if (set.Count == 0) trackMap.Remove(item.Track);
			}
		}

		internal void NotifyTrackSynced(ChartComponent track) => OnTrackSynced?.Invoke(track);
	}
}
