#nullable enable

using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
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
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<ChartComponent>(viewPool,
				ViewPoolRegistrar<ChartComponent>.RegisterTarget.Get,
				handler =>
				{
					var track = viewPool[handler]!;
					if (track.Model is not ITrack model) return;
					var presenter = handler.Script<T3TrackViewPresenter>();
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
					var presenter = handler.Script<T3TrackViewPresenter>();
					presenter.PositionModifier.Unregister(positionPriority, true);
				})
		};

		// Private
		private IViewPool<ChartComponent> viewPool = default!;

		// Constructor
		[Inject]
		private void Construct([Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.viewPool = viewPool;
		}
	}
}