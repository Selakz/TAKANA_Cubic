#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class NoteTimingBlockSystem : HierarchySystem<NoteTimingBlockSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<PropertyChangedEventHandler>(
				e => timingBlockClickTime.PropertyChanged += e,
				e => timingBlockClickTime.PropertyChanged -= e,
				(_, _) => OnTimingBlockClicked(timingBlockClickTime.Value))
		};

		// Private
		[Inject] private readonly NotifiableProperty<T3Time> timingBlockClickTime = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
		[Inject] private readonly ChartEditSystem chartEditSystem = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Defined Functions
		private void OnTimingBlockClicked(T3Time time)
		{
			var liveNotes = selectDataset
				.Where(c => T3ChartClassifier.Instance.IsOfType(c, T3Flag.Live | T3Flag.Note))
				.ToList();
			if (liveNotes.Count == 0) return;

			HashSet<(T3Flag, ChartComponent?)> seen = new();
			foreach (var note in liveNotes)
			{
				if (!seen.Add((T3ChartClassifier.Instance.Classify(note), note.Parent))) return;
			}

			List<ICommand> commands = new();
			foreach (var note in liveNotes)
			{
				ICommand? command = note.Model switch
				{
					Hit hit when time != hit.TimeJudge => chartEditSystem.NudgeNote(note, time - hit.TimeJudge),
					Hold hold when time != hold.TimeEnd => chartEditSystem.NudgeHoldEnd(note, time - hold.TimeEnd),
					_ => null
				};
				if (command is not null) commands.Add(command);
			}

			if (commands.Count > 0) commandManager.Add(new BatchCommand(commands, "Set Note End Time"));
		}
	}
}
