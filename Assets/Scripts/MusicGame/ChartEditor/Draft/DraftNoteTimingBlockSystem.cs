#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Draft
{
	public class DraftNoteTimingBlockSystem : HierarchySystem<DraftNoteTimingBlockSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<bool>(draftContainer.IsInDraftMode, isInDraftMode => IsEnabled = isInDraftMode)
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<PropertyChangedEventHandler>(
				e => timingBlockClickTime.PropertyChanged += e,
				e => timingBlockClickTime.PropertyChanged -= e,
				(_, _) => OnTimingBlockClicked(timingBlockClickTime.Value))
		};

		// Private
		[Inject] private readonly DraftContainer draftContainer = default!;
		[Inject] private readonly NotifiableProperty<T3Time> timingBlockClickTime = default!;
		[Inject] private readonly ChartSelectDataset selectDataset = default!;
		[Inject] private readonly ChartEditSystem chartEditSystem = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Defined Functions
		private void OnTimingBlockClicked(T3Time time)
		{
			var draftNotes = selectDataset
				.Where(c => T3ChartClassifier.Instance.IsOfType(c, T3Flag.Draft | T3Flag.Note))
				.ToList();
			if (draftNotes.Count == 0) return;

			for (int i = 0; i < draftNotes.Count; i++)
			{
				var flagI = T3ChartClassifier.Instance.Classify(draftNotes[i]);
				var solitaryI = (ISolitaryNote)draftNotes[i].Model;
				for (int j = i + 1; j < draftNotes.Count; j++)
				{
					if (flagI == T3ChartClassifier.Instance.Classify(draftNotes[j]) &&
					    draftNotes[j].Model is ISolitaryNote solitaryJ &&
					    Mathf.Approximately(solitaryI.Position, solitaryJ.Position) &&
					    Mathf.Approximately(solitaryI.Width, solitaryJ.Width))
					{
						return;
					}
				}
			}

			List<ICommand> commands = new();
			foreach (var note in draftNotes)
			{
				ICommand? command = note.Model switch
				{
					Hit hit when time != hit.TimeJudge => chartEditSystem.NudgeNote(note, time - hit.TimeJudge),
					Hold hold when time != hold.TimeEnd => chartEditSystem.NudgeHoldEnd(note, time - hold.TimeEnd),
					_ => null
				};
				if (command is not null) commands.Add(command);
			}

			if (commands.Count > 0) commandManager.Add(new BatchCommand(commands, "Set Draft Note End Time"));
		}
	}
}
