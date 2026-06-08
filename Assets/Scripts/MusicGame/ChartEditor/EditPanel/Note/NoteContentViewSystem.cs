#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.EditPanel.Note
{
	public class NoteContentViewSystem : HierarchySystem<NoteContentViewSystem>
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler =>
			{
				if (handler.TryScript<EditDraftNoteContent>() is { } content)
				{
					return new DraftNoteContentRegistrar(content,
						decoratorPool[handler]!, selectDataset, music, system, commandManager);
				}

				return new NoteContentRegistrar(handler.Script<EditNoteContent>(),
					decoratorPool[handler]!, selectDataset, music, system, commandManager);
			})
		};

		// Private
		[Inject, Key("note-decoration")] private IViewPool<ChartComponent> decoratorPool = default!;
		[Inject] private ChartSelectDataset selectDataset = default!;
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private ChartEditSystem system = default!;
		[Inject] private CommandManager commandManager = default!;
	}
}