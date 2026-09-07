#nullable enable

using System;
using DLCore.Models.Note;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace DLCore.Gameplay.Basic
{
	public class DLFreeNoteViewSystem : HierarchySystem<DLFreeNoteViewSystem>
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
					var component = viewPool[handler]!;
					if (component.Model is not IDLFreeNote freeNote) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					presenter.PositionModifier.Register(
						value => new(freeNote.HorizontalMovement.GetPos(music.ChartTime), value.y),
						positionPriority);
					Func<Vector2, Vector2> widthFunction = value =>
						new(freeNote.HorizontalMovement.GetWidth(music.ChartTime), value.y);
					foreach (var modifier in presenter.WidthModifiers)
					{
						modifier.Register(widthFunction, widthPriority);
					}
				},
				() =>
				{
					var component = viewPool[handler]!;
					if (component.Model is not IDLFreeNote) return;
					var presenter = handler.Script<T3NoteViewPresenter>();
					presenter.PositionModifier.Unregister(positionPriority, true);
					foreach (var modifier in presenter.WidthModifiers)
					{
						modifier.Unregister(widthPriority, true);
					}
				}))
		};

		// Private
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
		[Inject] private IGameAudioPlayer music = default!;
	}
}
