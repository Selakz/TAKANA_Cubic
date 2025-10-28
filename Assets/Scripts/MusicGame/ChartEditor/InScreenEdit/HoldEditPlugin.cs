#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using MusicGame.ChartEditor.Select;
using MusicGame.Components.Notes;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.Setting;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class HoldEditPlugin : MonoBehaviour
	{
		// Event Handlers
		private void HoldEndToNext()
		{
			var holds = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingHold);
			var noteToNextDistance = ISingletonSetting<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
			List<UpdateComponentArg> args = new();
			foreach (var editingHold in holds)
			{
				var hold = (editingHold as EditingHold)!.Hold;
				var actualDistance = Mathf.Min(hold.Parent.TimeEnd - hold.TimeEnd, noteToNextDistance);
				UpdateComponentArg arg = new(hold,
					h => (h as Hold)!.TimeEnd += actualDistance,
					h => (h as Hold)!.TimeEnd -= actualDistance);
				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void HoldEndToPrevious()
		{
			var holds = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingHold);
			var noteToPreviousDistance = ISingletonSetting<InScreenEditSetting>.Instance.NoteNudgeDistance.Value;
			List<UpdateComponentArg> args = new();
			foreach (var editingHold in holds)
			{
				var hold = (editingHold as EditingHold)!.Hold;
				var actualDistance = Mathf.Min(hold.TimeEnd - hold.TimeJudge - 1, noteToPreviousDistance);
				UpdateComponentArg arg = new(hold,
					h => (h as Hold)!.TimeEnd -= actualDistance,
					h => (h as Hold)!.TimeEnd += actualDistance);
				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void HoldEndToNextBeat()
		{
			if (InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever timeRetriever) return;
			var holds = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingHold);
			List<UpdateComponentArg> args = new();
			foreach (var editingHold in holds)
			{
				var hold = (editingHold as EditingHold)!.Hold;
				var updatedTime = Mathf.Min(hold.Parent.TimeEnd, timeRetriever.GetCeilTime(hold.TimeEnd));
				var actualDistance = updatedTime - hold.TimeEnd;
				UpdateComponentArg arg = new(hold,
					h => (h as Hold)!.TimeEnd += actualDistance,
					h => (h as Hold)!.TimeEnd -= actualDistance);
				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		private void HoldEndToPreviousBeat()
		{
			if (InScreenEditManager.Instance.TimeRetriever is not GridTimeRetriever timeRetriever) return;
			var holds = ISelectManager.Instance.SelectedTargets.Values.Where(note => note is EditingHold);
			List<UpdateComponentArg> args = new();
			foreach (var editingHold in holds)
			{
				var hold = (editingHold as EditingNote)!.Note;
				var updatedTime = Mathf.Max(hold.TimeJudge + 1, timeRetriever.GetFloorTime(hold.TimeEnd));
				var actualDistance = hold.TimeEnd - updatedTime;
				UpdateComponentArg arg = new(hold,
					h => (h as Hold)!.TimeEnd -= actualDistance,
					h => (h as Hold)!.TimeEnd += actualDistance);
				args.Add(arg);
			}

			if (args.Count > 0)
			{
				CommandManager.Instance.Add(new UpdateComponentsCommand(args));
			}
		}

		// System Functions
		void OnEnable()
		{
			InputManager.Instance.Register("InScreenEdit", "HoldEndToNext", _ => HoldEndToNext());
			InputManager.Instance.Register("InScreenEdit", "HoldEndToPrevious", _ => HoldEndToPrevious());
			InputManager.Instance.Register("InScreenEdit", "HoldEndToNextBeat", _ => HoldEndToNextBeat());
			InputManager.Instance.Register("InScreenEdit", "HoldEndToPreviousBeat", _ => HoldEndToPreviousBeat());
		}
	}
}