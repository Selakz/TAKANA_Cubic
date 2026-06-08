#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Models.Note;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteMirrorSystem : HierarchySystem<DraftNoteMirrorSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode, isInDraftMode => IsEnabled = isInDraftMode)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Mirror", NoteMirror),
			new InputRegistrar("InScreenEdit", "MirrorCopy", NoteMirrorCopy),
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private ChartSelectDataset dataset = default!;

		// Event Handlers
		private void NoteMirror()
		{
			var notes = dataset.Where(component => component.Model is ISolitaryNote);
			var command = new BatchCommand(
				notes.Select(note => new UpdateComponentCommand(note,
					model => (model as ISolitaryNote)!.Position *= -1,
					model => (model as ISolitaryNote)!.Position *= -1)),
				"Mirror Notes");
			CommandManager.Instance.Add(command);
		}

		private void NoteMirrorCopy()
		{
			var notes = dataset.Where(component => component.Model is ISolitaryNote).ToArray();
			IEnumerable<ICommand> cloneCommands = notes.Select(note => new CloneComponentCommand(note));
			IEnumerable<ICommand> mirrorCommands = notes.Select(note => new UpdateComponentCommand(note,
				model => (model as ISolitaryNote)!.Position *= -1,
				model => (model as ISolitaryNote)!.Position *= -1));
			var command = new BatchCommand(cloneCommands.Concat(mirrorCommands), "Mirror and Copy Notes");
			CommandManager.Instance.Add(command);
		}
	}
}