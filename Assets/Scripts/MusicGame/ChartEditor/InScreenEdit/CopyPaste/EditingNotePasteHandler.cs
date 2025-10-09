using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Components;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class EditingNotePasteHandler : IPasteHandler
	{
		public CopyPastePlugin CopyPaste { get; }

		public EditingNotePasteHandler(CopyPastePlugin copyPastePlugin)
		{
			CopyPaste = copyPastePlugin;
		}

		public string GetDescription()
		{
			int count = CopyPaste.Clipboard.Count(c => c is EditingNote);
			return $"已复制{count}个Note";
		}

		public void Cut()
		{
			var toDelete = CopyPaste.Clipboard.Where(c => c is EditingNote).ToList();
			if (toDelete.Count == 0) return;
			CommandManager.Instance.Add(new DeleteComponentsCommand(toDelete));
		}

		public bool Paste(out string message)
		{
			if (ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack)
			{
				message = "需选中一条轨道以进行Note普通粘贴";
				return false;
			}

			var notes = CopyPaste.Clipboard.Where(c => c is EditingNote).Cast<EditingNote>().ToList();
			if (notes.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			var track = editingTrack.Track;
			notes.Sort((a, b) => a.Note.TimeJudge.CompareTo(b.Note.TimeJudge));
			float baseTime = notes[0].Note.TimeJudge;

			List<IComponent> cloneComponents = new();
			var mousePosition = Input.mousePosition;
			var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(mousePosition);
			T3Time time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
			foreach (var editingNote in notes)
			{
				var note = editingNote.Note;
				var newTime = time + note.TimeJudge - baseTime;
				if (newTime < track.TimeInstantiate || newTime > track.TimeEnd)
				{
					message = "粘贴片段长度超出轨道时间范围";
					return false;
				}

				cloneComponents.Add(editingNote.Clone(newTime, track));
			}

			CommandManager.Instance.Add(new AddComponentsCommand(cloneComponents));
			message = "粘贴成功！";
			return true;
		}

		public bool ExactPaste(out string message)
		{
			if (ISelectManager.Instance.SelectedTargets.Values.Any(c => c is EditingTrack))
			{
				message = "需无选中的轨道以进行Note原位粘贴";
				return false;
			}

			var notes = CopyPaste.Clipboard.Where(c => c is EditingNote).Cast<EditingNote>().ToList();
			if (notes.Count == 0)
			{
				message = IPasteHandler.NoPasteObjectMessage;
				return false;
			}

			notes.Sort((a, b) => a.Note.TimeJudge.CompareTo(b.Note.TimeJudge));
			float baseTime = notes[0].Note.TimeJudge;

			List<IComponent> cloneComponents = new();
			var mousePosition = Input.mousePosition;
			var gamePoint = LevelManager.Instance.LevelCamera.ScreenToWorldPoint(mousePosition);
			T3Time time = InScreenEditManager.Instance.TimeRetriever.GetTimeStart(gamePoint);
			foreach (var editingNote in notes)
			{
				var note = editingNote.Note;
				var newTime = time + note.TimeJudge - baseTime;
				if (newTime < note.Parent.TimeInstantiate || newTime > note.Parent.TimeEnd)
				{
					message = "粘贴片段长度超出轨道时间范围";
					return false;
				}

				cloneComponents.Add(note.Clone(newTime, note.Parent));
			}

			CommandManager.Instance.Add(new AddComponentsCommand(cloneComponents));
			message = "粘贴成功！";
			return true;
		}
	}
}