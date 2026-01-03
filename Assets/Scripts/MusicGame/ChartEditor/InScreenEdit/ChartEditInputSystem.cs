#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class ChartEditInputSystem : T3System
	{
		private readonly CommandManager commandManager;
		private readonly Camera camera;
		private readonly NotifiableProperty<ITimeRetriever> timeRetriever;
		private readonly NotifiableProperty<IWidthRetriever> widthRetriever;
		private readonly ChartEditSystem system;
		private readonly ChartSelectDataset dataset;
		private readonly int chartEditPriority;
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		public NotifiableProperty<T3Flag> NoteType { get; } = new(T3Flag.Tap);

		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Create", chartEditPriority,
				() =>
				{
					var mousePoint = Input.mousePosition;
					if (!camera.ContainsScreenPoint(mousePoint) ||
					    !camera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
					{
						return true;
					}

					var time = timeRetriever.Value.GetTimeStart(gamePoint);
					var command = NoteType.Value switch
					{
						T3Flag.Tap => system.CreateHit(HitType.Tap, time),
						T3Flag.Slide => system.CreateHit(HitType.Slide, time),
						T3Flag.Hold => system.CreateHold(time, timeRetriever.Value.GetHoldTimeEnd(gamePoint)),
						_ => EmptyCommand.Instance
					};
					if (command is not null) commandManager.Add(command);
					return false;
				}),
			new InputRegistrar("InScreenEdit", "CreateTrack",
				() =>
				{
					var mousePoint = Input.mousePosition;
					if (!camera.ContainsScreenPoint(mousePoint) ||
					    !camera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint)) return;

					var timeStart = timeRetriever.Value.GetTimeStart(gamePoint);
					var timeEnd = timeRetriever.Value.GetTrackTimeEnd(gamePoint);
					float width = widthRetriever.Value.GetWidth(gamePoint);
					float position = widthRetriever.Value.GetPosition(gamePoint);
					float left = position - width / 2, right = position + width / 2;
					var command = system.CreateTrack(timeStart, timeEnd, left, right);
					if (command is not null) commandManager.Add(command);
				}),
			new InputRegistrar("InScreenEdit", "Delete", "delete", chartEditPriority,
				() =>
				{
					var mousePoint = Input.mousePosition;
					if (!camera.ContainsScreenPoint(mousePoint) ||
					    !camera.ScreenToWorldPoint(gamePlane, mousePoint, out _)) return true;
					var command = system.DeleteSelected();
					if (command is not null) commandManager.Add(command);
					return false;
				}),
			new InputRegistrar("InScreenEdit", "ToNext", "toNext", chartEditPriority,
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

					if (commands.Any())
					{
						commandManager.Add(new BatchCommand(commands, "Note to next"));
						return false;
					}

					return true;
				}),
			new InputRegistrar("InScreenEdit", "ToPrevious", "toPrevious", chartEditPriority,
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

					if (commands.Any())
					{
						commandManager.Add(new BatchCommand(commands, "Note to previous"));
						return false;
					}

					return true;
				}),
			new InputRegistrar("InScreenEdit", "ToNextBeat", "toNextBeat", chartEditPriority,
				() =>
				{
					if (timeRetriever.Value is not GridTimeRetriever retriever) return true;
					var notes = dataset.Where(c => c.Model is INote);
					List<ICommand> commands = new();
					foreach (var note in notes)
					{
						var distance = retriever.GetCeilTime(note.Model.TimeMin) - note.Model.TimeMin;
						var command = system.NudgeNote(note, distance);
						if (command is not null) commands.Add(command);
					}

					if (commands.Any())
					{
						commandManager.Add(new BatchCommand(commands, "Note to next beat"));
						return false;
					}

					return true;
				}),
			new InputRegistrar("InScreenEdit", "ToPreviousBeat", "toPreviousBeat", chartEditPriority,
				() =>
				{
					if (timeRetriever.Value is not GridTimeRetriever retriever) return true;
					var notes = dataset.Where(c => c.Model is INote);
					List<ICommand> commands = new();
					foreach (var note in notes)
					{
						var distance = retriever.GetFloorTime(note.Model.TimeMin) - note.Model.TimeMin;
						var command = system.NudgeNote(note, distance);
						Debug.Log(retriever.GetFloorTime(note.Model.TimeMin) + ": " + note.Parent!.Model.TimeMin);
						if (command is not null) commands.Add(command);
					}

					if (commands.Any())
					{
						commandManager.Add(new BatchCommand(commands, "Note to previous beat"));
						return false;
					}

					return true;
				})
		};

		public ChartEditInputSystem(
			CommandManager commandManager,
			[Key("stage")] Camera camera,
			NotifiableProperty<ITimeRetriever> timeRetriever,
			NotifiableProperty<IWidthRetriever> widthRetriever,
			ChartEditSystem system,
			ChartSelectDataset dataset,
			int chartEditPriority) : base(true)
		{
			this.commandManager = commandManager;
			this.camera = camera;
			this.timeRetriever = timeRetriever;
			this.widthRetriever = widthRetriever;
			this.system = system;
			this.dataset = dataset;
			this.chartEditPriority = chartEditPriority;
		}
	}
}