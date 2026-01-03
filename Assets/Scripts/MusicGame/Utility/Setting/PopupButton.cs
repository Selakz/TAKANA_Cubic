#nullable enable

using T3Framework.Runtime.Input;
using T3Framework.Static;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.Utility.Setting
{
	public class PopupButton : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private bool isOpen = true;
		[SerializeField] private Button popupButton = default!;
		[SerializeField] private GameObject popupObject = default!;

		// Event Handlers
		private void OnPopupButtonClick()
		{
			popupObject.SetActive(isOpen);
			ISingleton<InputManager>.Instance.GlobalInputEnabled.Value = !isOpen;
		}

		// System Functions
		void Awake()
		{
			popupButton.onClick.AddListener(OnPopupButtonClick);
		}

		void OnDestroy()
		{
			popupButton.onClick.RemoveListener(OnPopupButtonClick);
		}
	}
}