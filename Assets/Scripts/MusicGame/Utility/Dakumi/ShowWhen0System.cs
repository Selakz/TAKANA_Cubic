#nullable enable

using MusicGame.Gameplay.Basic;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Stage;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Utility.Dakumi
{
	public class ShowWhen0System : HierarchySystem<ShowWhen0System>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority positionPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<GameplayStageSkinConfig>(service.OnStageReset,
				config => IsEnabled = config.trackBehaviour is TrackBehaviour.Instant)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<ChartComponent>(viewPool,
				ViewPoolRegistrar<ChartComponent>.RegisterTarget.Get,
				handler =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack model) return;
					var presenter = handler.Script<ITrackViewPresenter>();
					presenter.PositionModifier.Register(
						position => Mathf.Approximately(presenter.WidthModifier.Value, 0) && !model.IsShowWhen0()
							? 100
							: position,
						positionPriority, true);
				}),
			new ViewPoolRegistrar<ChartComponent>(viewPool,
				ViewPoolRegistrar<ChartComponent>.RegisterTarget.Release,
				handler =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack) return;
					var presenter = handler.Script<ITrackViewPresenter>();
					presenter.PositionModifier.Unregister(positionPriority, true);
				})
		};

		// Private
		[Inject] private IStageViewGenerateService service = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}