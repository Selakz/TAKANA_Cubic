#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3HoldLengthSystem : HierarchySystem<T3HoldLengthSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority lengthPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					var note = viewPool[handler]!;
					if (note.Model is not Hold hold) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					foreach (var modifier in presenter.HeightModifiers)
					{
						modifier.Register(value => new(value.x,
								hold.TailMovement.GetPos(music.ChartTime) - hold.Movement.GetPos(music.ChartTime)),
							lengthPriority, true);
					}
				},
				() =>
				{
					var note = viewPool[handler]!;
					if (note.Model is not Hold) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					foreach (var modifier in presenter.HeightModifiers)
					{
						modifier.Unregister(lengthPriority, true);
					}
				}))
		};

		// Private
		[Inject] private IGameAudioPlayer music = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}