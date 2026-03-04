#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class NoteEditInputSystem : HierarchySystem<NoteEditInputSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Create",
				() =>
				{
					List<ICommand> commands = new();
					foreach (var info in rawDataset)
					{
						if (service.IsValid(info) is { } error)
						{
							T3Logger.Log("Notice", error, T3LogType.Warn);
							return;
						}

						var model = info.GenerateModel();
						if (info.Parent.Value is { BelongingChart: { } chart } parent)
							commands.Add(new AddComponentCommand(chart, model, parent));
					}

					commandManager.Add(new BatchCommand(commands, "Create"));
				}),
			new InputRegistrar("InScreenEdit", "ToNext",
				() =>
				{
					var notes = dataset.Where(c => c.Model is INote);
					var distance = ISingleton<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
					List<ICommand> commands = new();
					foreach (var note in notes)
					{
						var command = system.NudgeNote(note, distance);
						if (command is not null) commands.Add(command);
					}

					commandManager.Add(new BatchCommand(commands, "Note to next"));
				}),
			new InputRegistrar("InScreenEdit", "ToPrevious",
				() =>
				{
					var notes = dataset.Where(c => c.Model is INote);
					var distance = ISingleton<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
					List<ICommand> commands = new();
					foreach (var note in notes)
					{
						var command = system.NudgeNote(note, -distance);
						if (command is not null) commands.Add(command);
					}

					commandManager.Add(new BatchCommand(commands, "Note to previous"));
				}),
			new InputRegistrar("InScreenEdit", "ToNextBeat",
				() =>
				{
					if (timeRetriever.TimeRetriever.Value is not GridTimeRetriever retriever) return;
					var notes = dataset.Where(c => c.Model is INote);
					List<ICommand> commands = new();
					foreach (var note in notes)
					{
						var distance = retriever.GetCeilTime(note.Model.TimeMin) - note.Model.TimeMin;
						var command = system.NudgeNote(note, distance);
						if (command is not null) commands.Add(command);
					}

					commandManager.Add(new BatchCommand(commands, "Note to next beat"));
				}),
			new InputRegistrar("InScreenEdit", "ToPreviousBeat",
				() =>
				{
					if (timeRetriever.TimeRetriever.Value is not GridTimeRetriever retriever) return;
					var notes = dataset.Where(c => c.Model is INote);
					List<ICommand> commands = new();
					foreach (var note in notes)
					{
						var distance = retriever.GetFloorTime(note.Model.TimeMin) - note.Model.TimeMin;
						var command = system.NudgeNote(note, distance);
						Debug.Log(retriever.GetFloorTime(note.Model.TimeMin) + ": " + note.Parent!.Model.TimeMin);
						if (command is not null) commands.Add(command);
					}

					commandManager.Add(new BatchCommand(commands, "Note to previous beat"));
				})
		};

		// Private
		private CommandManager commandManager = default!;
		private IDataset<NoteRawInfo> rawDataset = default!;
		private StageMouseTimeRetriever timeRetriever = default!;
		private ChartEditSystem system = default!;
		private ChartSelectDataset dataset = default!;
		private INoteRawInfoService service = default!;

		// Constructor
		[Inject]
		private void Construct(
			CommandManager commandManager,
			IDataset<NoteRawInfo> rawDataset,
			StageMouseTimeRetriever timeRetriever,
			ChartEditSystem system,
			ChartSelectDataset dataset,
			INoteRawInfoService service)
		{
			this.commandManager = commandManager;
			this.rawDataset = rawDataset;
			this.timeRetriever = timeRetriever;
			this.system = system;
			this.dataset = dataset;
			this.service = service;
		}
	}
}