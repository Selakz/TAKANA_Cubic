#nullable enable

using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Speed
{
	public class NoteSpeedSystem : HierarchySystem<NoteSpeedSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority positionPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					if (handler.TryScript<T3NoteViewPresenter>() is not { } presenter) return;
					presenter.PositionModifier.Register(
						value => new(value.x, value.y * speed.Value.SpeedRate),
						positionPriority);
				},
				() =>
				{
					if (handler.TryScript<T3NoteViewPresenter>() is not { } presenter) return;
					presenter.PositionModifier.Unregister(positionPriority, true);
				}))
		};

		// Private
		[Inject] private NotifiableProperty<ISpeed> speed = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}