#nullable enable

using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Decoration.Track
{
	public class TrackSourceSystem : HierarchySystem<TrackSourceSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DatasetLifetimeRegistrar<ChartComponent>(selectDataset,
				component => new CustomRegistrar(
					() =>
					{
						if (component.Model is ITrack) trackDataset.Add(component);
					},
					() =>
					{
						if (component.Model is ITrack) trackDataset.Remove(component);
					}), true)
		};

		// Private
		[Inject, Key("track-decoration")] private readonly IDataset<ChartComponent> trackDataset = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
	}
}