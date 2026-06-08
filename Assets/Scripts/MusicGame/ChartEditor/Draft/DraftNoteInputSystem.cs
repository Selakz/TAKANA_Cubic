#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Draft.Commands;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Models.Note;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteInputSystem : HierarchySystem<DraftNoteInputSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("Draft", "ToggleDraftMode",
				() => draftContainer.IsInDraftMode.Value = !draftContainer.IsInDraftMode.Value),
			new InputRegistrar("InScreenEdit", "Widen", () =>
			{
				if (dataset.Count == 0) return;
				var increment = ISingleton<DraftSetting>.Instance.DraftNoteWidthIncrement;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var widthBefore = ((ISolitaryNote)note.Model).Width;
					commands.Add(new UpdateComponentCommand(note,
						_ => ((ISolitaryNote)note.Model).Width = widthBefore + increment,
						_ => ((ISolitaryNote)note.Model).Width = widthBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Widen draft notes"));
			}),
			new InputRegistrar("InScreenEdit", "WidenToGrid", () =>
			{
				if (dataset.Count == 0) return;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var model = (ISolitaryNote)note.Model;
					var widthBefore = model.Width;
					var widthAfter = draftNoteService.GetWidenedGridWidth(model.Position, widthBefore);
					commands.Add(new UpdateComponentCommand(note,
						_ => model.Width = widthAfter,
						_ => model.Width = widthBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Widen draft notes to grid"));
			}),
			new InputRegistrar("InScreenEdit", "Narrow", () =>
			{
				if (dataset.Count == 0) return;
				var increment = ISingleton<DraftSetting>.Instance.DraftNoteWidthIncrement;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var widthBefore = ((ISolitaryNote)note.Model).Width;
					commands.Add(new UpdateComponentCommand(note,
						_ => ((ISolitaryNote)note.Model).Width = widthBefore - increment,
						_ => ((ISolitaryNote)note.Model).Width = widthBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Narrow draft notes"));
			}),
			new InputRegistrar("InScreenEdit", "NarrowToGrid", () =>
			{
				if (dataset.Count == 0) return;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var model = (ISolitaryNote)note.Model;
					var widthBefore = model.Width;
					var widthAfter = draftNoteService.GetNarrowedGridWidth(model.Position, widthBefore);
					commands.Add(new UpdateComponentCommand(note,
						_ => model.Width = widthAfter,
						_ => model.Width = widthBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Narrow draft notes to grid"));
			}),
			new InputRegistrar("InScreenEdit", "ToLeft", () =>
			{
				if (dataset.Count == 0) return;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var model = (ISolitaryNote)note.Model;
					var positionBefore = model.Position;
					var positionAfter =
						model.Position - ISingleton<DraftSetting>.Instance.DraftNotePositionNudgeDistance;
					commands.Add(new UpdateComponentCommand(note,
						_ => model.Position = positionAfter,
						_ => model.Position = positionBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Move draft notes to left"));
			}),
			new InputRegistrar("InScreenEdit", "ToRight", () =>
			{
				if (dataset.Count == 0) return;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var model = (ISolitaryNote)note.Model;
					var positionBefore = model.Position;
					var positionAfter =
						model.Position + ISingleton<DraftSetting>.Instance.DraftNotePositionNudgeDistance;
					commands.Add(new UpdateComponentCommand(note,
						_ => model.Position = positionAfter,
						_ => model.Position = positionBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Move draft notes to right"));
			}),
			new InputRegistrar("InScreenEdit", "ToLeftGrid", () =>
			{
				if (dataset.Count == 0) return;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var model = (ISolitaryNote)note.Model;
					var positionBefore = model.Position;
					var positionAfter = draftNoteService.GetLeftAttachedPosition(positionBefore);
					commands.Add(new UpdateComponentCommand(note,
						_ => model.Position = positionAfter,
						_ => model.Position = positionBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Move draft notes to left grid"));
			}),
			new InputRegistrar("InScreenEdit", "ToRightGrid", () =>
			{
				if (dataset.Count == 0) return;
				List<ICommand> commands = new();
				foreach (var note in dataset.Where(c => c.Model is ISolitaryNote))
				{
					var model = (ISolitaryNote)note.Model;
					var positionBefore = model.Position;
					var positionAfter = draftNoteService.GetRightAttachedPosition(positionBefore);
					commands.Add(new UpdateComponentCommand(note,
						_ => model.Position = positionAfter,
						_ => model.Position = positionBefore));
				}

				commandManager.Add(new BatchCommand(commands, "Move draft notes to right grid"));
			}),
			new InputRegistrar("HoldEdit", "CreateHoldBetween", () =>
			{
				if (dataset.Count != 2) return;
				var notes = dataset.ToArray();
				var command = new CreateDraftHoldBetweenCommand(notes[0], notes[1]);
				if (command.SetInit()) commandManager.Add(command);
			})
		};

		// Private
		[Inject] private DraftContainer draftContainer = default!;
		[Inject] private CommandManager commandManager = default!;
		[Inject] private ChartSelectDataset dataset = default!;
		[Inject] private IDraftNoteService draftNoteService = default!;
	}
}