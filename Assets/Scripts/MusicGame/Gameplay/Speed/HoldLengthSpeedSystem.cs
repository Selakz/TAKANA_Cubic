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
	public class HoldLengthSpeedSystem : HierarchySystem<HoldLengthSpeedSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority lengthPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					if (handler.TryScript<T3NoteViewPresenter>() is not { } presenter) return;
					foreach (var modifier in presenter.HeightModifiers)
					{
						modifier.Register(
							value => new(value.x, value.y * speed.Value.SpeedRate),
							lengthPriority);
					}
				},
				() =>
				{
					if (handler.TryScript<T3NoteViewPresenter>() is not { } presenter) return;
					foreach (var modifier in presenter.HeightModifiers)
					{
						modifier.Unregister(lengthPriority, true);
					}
				}))
		};

		// Private
		[Inject] private NotifiableProperty<ISpeed> speed = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}