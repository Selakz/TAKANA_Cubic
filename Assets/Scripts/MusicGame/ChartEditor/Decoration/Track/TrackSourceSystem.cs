#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackSourceSystem : T3System
	{
		private readonly IDataset<ChartComponent> trackDataset;
		private readonly ChartSelectDataset selectDataset;

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new DatasetRegistrar<ChartComponent>(selectDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataAdded, component =>
				{
					if (component.Model is ITrack) trackDataset.Add(component);
				}),
			new DatasetRegistrar<ChartComponent>(selectDataset,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataRemoved, component =>
				{
					if (component.Model is ITrack) trackDataset.Remove(component);
				})
		};

		public TrackSourceSystem(
			[Key("track-decoration")] IDataset<ChartComponent> trackDataset,
			ChartSelectDataset selectDataset) : base(true)
		{
			this.trackDataset = trackDataset;
			this.selectDataset = selectDataset;
		}
	}
}