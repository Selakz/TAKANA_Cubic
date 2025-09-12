#nullable enable

using T3Framework.Runtime.Input;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class SwitchNoteTypePlugin : MonoBehaviour
	{
		// Event Handlers
		private void SwitchNoteType()
		{
			var newNoteType = InScreenEditManager.Instance.NoteType switch
			{
				InScreenEditManager.CreateNoteType.Tap => InScreenEditManager.CreateNoteType.Slide,
				InScreenEditManager.CreateNoteType.Slide => InScreenEditManager.CreateNoteType.Hold,
				InScreenEditManager.CreateNoteType.Hold => InScreenEditManager.CreateNoteType.Tap,
				_ => InScreenEditManager.CreateNoteType.Tap
			};
			InScreenEditManager.Instance.NoteType = newNoteType;
		}

		// System Functions
		void OnEnable()
		{
			InputManager.Instance.Register("InScreenEdit", "SwitchCreateType", _ => SwitchNoteType());
		}
	}
}