#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Level;
using MusicGame.ChartEditor.Select;
using T3Framework.Runtime.Extensions;
using T3Framework.Runtime.Input;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class DummyTogglePlugin : MonoBehaviour
	{
		// Event Handlers
		private void ToggleDummyState()
		{
			var notes = ISelectManager.Instance.SelectedTargets.Values
				.Where(component => component is EditingNote).Cast<EditingNote>();
			foreach (var note in notes)
			{
				note.Note.Properties["isDummy"] = !note.Note.Properties.Get("isDummy", false);
			}

			IEditingChartManager.Instance.UpdateProperties();
		}

		// System Functions
		void OnEnable()
		{
			InputManager.Instance.Register("InScreenEdit", "ToggleDummy", _ => ToggleDummyState());
		}
	}
}