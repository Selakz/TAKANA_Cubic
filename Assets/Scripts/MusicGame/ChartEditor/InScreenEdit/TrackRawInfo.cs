#nullable enable

using System;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class TrackRawInfo : IComponent
	{
		public ChartComponent Track { get; }

		public NotifiableProperty<ChartComponent?> Parent { get; }

		public TrackRawInfo(ChartComponent track, ChartComponent? parent)
		{
			Track = track;
			Parent = new(parent);
		}

		public event EventHandler? OnComponentUpdated;

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public static TrackRawInfo? FromComponent(ChartComponent track)
		{
			if (track.Model is not ITrack) return null;
			var token = track.GetSerializationToken();
			var cloned = ChartComponent.Deserialize(token, null!);
			return new TrackRawInfo(cloned, track.Parent);
		}
	}
}