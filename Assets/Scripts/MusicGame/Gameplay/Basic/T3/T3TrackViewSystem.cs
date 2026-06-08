#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3TrackViewSystem : HierarchySystem<T3TrackViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority widthPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack trackModel) return;
					var presenter = handler.Script<T3TrackViewPresenter>();
					presenter.PositionModifier.Register(
						_ => trackModel.Movement.GetPos(music.ChartTime), positionPriority, true);
					presenter.WidthModifier.Register(
						_ => trackModel.Movement.GetWidth(music.ChartTime), widthPriority, true);
				},
				() =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack) return;
					var presenter = handler.Script<T3TrackViewPresenter>();
					presenter.PositionModifier.Unregister(positionPriority, true);
					presenter.WidthModifier.Unregister(widthPriority, true);
				}))
		};

		// Private
		[Inject] private IGameAudioPlayer music = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}