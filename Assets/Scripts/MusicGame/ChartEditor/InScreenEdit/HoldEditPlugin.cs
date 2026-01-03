#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class HoldEditPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("HoldEdit", "HoldEndToNext", HoldEndToNext),
			new InputRegistrar("HoldEdit", "HoldEndToPrevious", HoldEndToPrevious),
			new InputRegistrar("HoldEdit", "HoldEndToNextBeat", HoldEndToNextBeat),
			new InputRegistrar("HoldEdit", "HoldEndToPreviousBeat", HoldEndToPreviousBeat),
			new InputRegistrar("HoldEdit", "CreateHoldBetween", CreateHoldBetween),
		};

		// Private
		private NotifiableProperty<ITimeRetriever> timeRetriever = default!;
		private ChartEditSystem system = default!;
		private ChartSelectDataset dataset = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			NotifiableProperty<ITimeRetriever> timeRetriever,
			ChartEditSystem system,
			ChartSelectDataset dataset)
		{
			this.timeRetriever = timeRetriever;
			this.system = system;
			this.dataset = dataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void HoldEndToNext()
		{
			List<ICommand> commands = new();
			var noteToNextDistance = ISingletonSetting<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
			foreach (var component in dataset)
			{
				var timeEnd = Mathf.Min(
					component.Parent?.Model.TimeMax ?? T3Time.MaxValue,
					component.Model.TimeMax + noteToNextDistance);
				if (system.NudgeHoldEnd(component, timeEnd - component.Model.TimeMax) is { } command)
				{
					commands.Add(command);
				}
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "HoldEndToNext"));
		}

		private void HoldEndToPrevious()
		{
			List<ICommand> commands = new();
			var noteToPreviousDistance = ISingletonSetting<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
			foreach (var component in dataset)
			{
				var timeEnd = Mathf.Max(
					component.Model.TimeMin + 1,
					component.Model.TimeMax - noteToPreviousDistance);
				if (system.NudgeHoldEnd(component, timeEnd - component.Model.TimeMax) is { } command)
				{
					commands.Add(command);
				}
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "HoldEndToPrevious"));
		}

		private void HoldEndToNextBeat()
		{
			if (timeRetriever.Value is not GridTimeRetriever retriever) return;
			List<ICommand> commands = new();
			foreach (var component in dataset)
			{
				var timeEnd = Mathf.Min(
					component.Parent?.Model.TimeMax ?? T3Time.MaxValue,
					retriever.GetCeilTime(component.Model.TimeMax));
				if (system.NudgeHoldEnd(component, timeEnd - component.Model.TimeMax) is { } command)
				{
					commands.Add(command);
				}
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "HoldEndToNextBeat"));
		}

		private void HoldEndToPreviousBeat()
		{
			if (timeRetriever.Value is not GridTimeRetriever retriever) return;
			List<ICommand> commands = new();
			foreach (var component in dataset)
			{
				var timeEnd = Mathf.Max(
					component.Model.TimeMin + 1,
					retriever.GetFloorTime(component.Model.TimeMax));
				if (system.NudgeHoldEnd(component, timeEnd - component.Model.TimeMax) is { } command)
				{
					commands.Add(command);
				}
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "HoldEndToPreviousBeat"));
		}

		private void CreateHoldBetween()
		{
			if (dataset.Count != 2) return;
			var notes = dataset.ToArray();

			var command = new CreateHoldBetweenCommand(notes[0], notes[1]);
			if (!command.SetInit()) return;
			CommandManager.Instance.Add(command);
		}
	}
}