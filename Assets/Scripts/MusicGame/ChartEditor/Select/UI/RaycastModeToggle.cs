#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Select.UI
{
	public class RaycastModeToggle : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private Toggle raycastModeToggle = default!;
		[SerializeField] private RaycastMode raycastMode;

		// Event Handlers
		private void OnRaycastModeToggleValueChanged(bool isOn)
		{
			if (!isOn) return;
			SelectRaycaster.Instance.RaycastMode = raycastMode;
		}

		// System Functions
		void Awake()
		{
			raycastModeToggle.onValueChanged.AddListener(OnRaycastModeToggleValueChanged);
		}
	}
}