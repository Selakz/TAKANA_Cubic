using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class CreateNoteTypeToggle : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Toggle toggle;
		[SerializeField] private InScreenEditManager.CreateNoteType noteType;

		// Event Handlers
		private void OnToggleValueChanged(bool isOn)
		{
			if (!isOn) return;
			InScreenEditManager.Instance.NoteType = noteType;
		}

		// System Functions
		void Awake()
		{
			toggle.onValueChanged.AddListener(OnToggleValueChanged);
		}

		void Update()
		{
			toggle.SetIsOnWithoutNotify(InScreenEditManager.Instance.NoteType == noteType);
		}
	}
}