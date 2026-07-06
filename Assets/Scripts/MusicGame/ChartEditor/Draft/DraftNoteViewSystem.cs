#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteViewSystem : HierarchySystem<DraftNoteViewSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority draftNoteOpacityPriority = default!;
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority widthPriority = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(viewPool, handler => new CustomRegistrar(
				() =>
				{
					if (viewPool[handler]!.Model is not ISolitaryNote note) return;
					var presenter = handler.Script<T3NoteViewPresenter>();

					presenter.PositionModifier.Register(
						_ => new(note.Position, note.Movement.GetPos(music.ChartTime)), positionPriority, true);
					foreach (var modifier in presenter.WidthModifiers)
					{
						modifier.Register(value => new(note.Width, value.y), widthPriority, true);
					}

					foreach (var cm in presenter.ColorModifiers)
						cm.Register(
							color => color with
							{
								a = draftContainer.IsInDraftMode.Value
									? color.a
									: ISingleton<DraftSetting>.Instance.DraftNoteOpacityInNormalMode.Value
							},
							draftNoteOpacityPriority, true);
				},
				() =>
				{
					if (viewPool[handler]!.Model is not ISolitaryNote note) return;
					var presenter = handler.Script<T3NoteViewPresenter>();

					presenter.PositionModifier.Unregister(positionPriority, true);
					foreach (var modifier in presenter.WidthModifiers)
					{
						modifier.Unregister(widthPriority, true);
					}

					foreach (var cm in presenter.ColorModifiers)
						cm.Unregister(draftNoteOpacityPriority, true);
				}))
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject, Key("stage")] private IViewPool<ChartComponent> viewPool = default!;
	}
}