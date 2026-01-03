#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Event;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class EditingNotePasteHandler : IPasteHandler
	{
		private readonly Camera levelCamera;
		private readonly ChartSelectDataset dataset;
		private readonly NotifiableProperty<ITimeRetriever> timeRetriever;
		private readonly Plane gamePlane = new(Vector3.forward, Vector3.zero);

		public CopyPastePlugin CopyPaste { get; }

		public EditingNotePasteHandler(
			CopyPastePlugin copyPastePlugin,
			Camera levelCamera,
			ChartSelectDataset dataset,
			NotifiableProperty<ITimeRetriever> timeRetriever)
		{
			CopyPaste = copyPastePlugin;
			this.levelCamera = levelCamera;
			this.dataset = dataset;
			this.timeRetriever = timeRetriever;
		}

		public string GetDescription()
		{
			int count = CopyPaste.Clipboard.Count(c => c.Model is INote);
			return $"Edit_CopyPaste_NoteClipboard|{count}";
		}

		public void Cut()
		{
			var toDelete = CopyPaste.Clipboard.Where(c => c.Model is INote).ToList();
			if (toDelete.Count == 0) return;
			CommandManager.Instance.Add(new BatchCommand(
				toDelete.Select(c => new DeleteComponentCommand(c)),
				"NoteCut"));
		}

		public bool Paste(out string message)
		{
			var mousePoint = Input.mousePosition;
			if (!levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				message = string.Empty;
				return false;
			}

			if (dataset.CurrentSelecting.Value is not { Model: ITrack } track)
			{
				message = "Edit_CopyPaste_PasteFailForTrack";
				return false;
			}

			if (track.GetLayerInfo()?.IsDecoration ?? false)
			{
				message = "Edit_CreateFailForDecoration";
				return false;
			}

			var notes = CopyPaste.Clipboard.Where(c => c.Model is INote).ToList();
			if (notes.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			notes.Sort((a, b) => ((INote)a.Model).TimeJudge.CompareTo(((INote)b.Model).TimeJudge));
			T3Time baseTime = ((INote)notes[0].Model).TimeJudge;

			var commands = new List<ICommand>();
			T3Time time = timeRetriever.Value.GetTimeStart(gamePoint);
			var distance = time - baseTime;
			foreach (var note in notes)
			{
				var model = (INote)note.Model;
				if (!notes[0].IsWithinParentRange(distance + model.TimeMin - notes[0].Model.TimeMin))
				{
					message = "Edit_NoteTimeOutOfRange";
					return false;
				}

				commands.Add(new CloneComponentCommand(note, component =>
				{
					component.Parent = dataset.CurrentSelecting.Value;
					component.Nudge(distance);
				}));
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "NotePaste"));
			message = "Edit_CopyPaste_PasteSuccess";
			return true;
		}

		public bool ExactPaste(out string message)
		{
			var mousePoint = Input.mousePosition;
			if (!levelCamera.ScreenToWorldPoint(gamePlane, mousePoint, out var gamePoint))
			{
				message = string.Empty;
				return false;
			}

			var notes = CopyPaste.Clipboard.Where(c => c.Model is INote).ToList();
			if (notes.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			notes.Sort((a, b) => ((INote)a.Model).TimeJudge.CompareTo(((INote)b.Model).TimeJudge));
			T3Time baseTime = ((INote)notes[0].Model).TimeJudge;

			var commands = new List<ICommand>();
			T3Time time = timeRetriever.Value.GetTimeStart(gamePoint);
			var distance = time - baseTime;
			foreach (var note in notes)
			{
				if (!note.IsWithinParentRange(distance))
				{
					message = "Edit_NoteTimeOutOfRange";
					return false;
				}

				commands.Add(new CloneComponentCommand(note, component =>
				{
					component.Parent = note.Parent;
					component.Nudge(distance);
				}));
			}

			CommandManager.Instance.Add(new BatchCommand(commands, "NoteExactPaste"));
			message = "Edit_CopyPaste_PasteSuccess";
			return true;
		}
	}
}