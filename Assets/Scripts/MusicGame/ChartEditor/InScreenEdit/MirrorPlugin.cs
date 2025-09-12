#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Select;
using MusicGame.Components;
using MusicGame.Components.Movement;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class MirrorPlugin : MonoBehaviour
	{
		// Static
		private static void Mirror(EditingTrack editingTrack)
		{
			var track = editingTrack.Track;
			if (track.Movement.Movement1 is V1EMoveList moveList1)
			{
				var moveItems = moveList1.ToList<V1EMoveItem>();
				for (var i = 0; i < moveItems.Count; i++)
				{
					moveItems[i] = new V1EMoveItem(moveItems[i].Time, -moveItems[i].Position, moveItems[i].Ease);
				}

				foreach (var moveItem in moveItems)
				{
					moveList1.Insert(moveItem);
				}
			}

			if (track.Movement.Movement2 is V1EMoveList moveList2)
			{
				var moveItems = moveList2.ToList<V1EMoveItem>();
				for (var i = 0; i < moveItems.Count; i++)
				{
					moveItems[i] = new V1EMoveItem(moveItems[i].Time, -moveItems[i].Position, moveItems[i].Ease);
				}

				foreach (var moveItem in moveItems)
				{
					moveList2.Insert(moveItem);
				}
			}

			(track.Movement.Movement1, track.Movement.Movement2) = (track.Movement.Movement2, track.Movement.Movement1);
		}

		// Event Handlers
		private void TrackMirror()
		{
			if (ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack) return;
			if (!EventManager.Instance.InvokeVeto("Edit_QueryMirror", out _)) return;
			var command = new UpdateComponentsCommand(new UpdateComponentArg(
				editingTrack,
				track => Mirror((track as EditingTrack)!),
				track => Mirror((track as EditingTrack)!)
			));
			CommandManager.Instance.Add(command);
		}

		private void TrackMirrorCopy()
		{
			if (ISelectManager.Instance.CurrentSelecting is not EditingTrack editingTrack) return;
			var newTrack = editingTrack.Track.Clone(editingTrack.Track.TimeInstantiate, 0);
			var newEditingTrack = new EditingTrack(newTrack)
			{
				Properties = new JObject(editingTrack.Properties)
			};
			Mirror(newEditingTrack);
			List<IComponent> cloneComponents = new() { newEditingTrack };
			if (IEditingChartManager.Instance != null)
			{
				var children = IEditingChartManager.Instance.GetChildrenComponents(editingTrack.Id);
				foreach (var note in children.Where(c => c is EditingNote).Cast<EditingNote>())
				{
					var newTime = note.Note.TimeJudge;
					var newNote = note.Note.Clone(newTime, newTrack);
					cloneComponents.Add(newNote);
				}
			}

			var command = new AddComponentsCommand(cloneComponents);
			CommandManager.Instance.Add(command);
		}

		// System Functions
		private void OnEnable()
		{
			InputManager.Instance.Register("InScreenEdit", "Mirror", _ => TrackMirror());
			InputManager.Instance.Register("InScreenEdit", "MirrorCopy", _ => TrackMirrorCopy());
		}
	}
}