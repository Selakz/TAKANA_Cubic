using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace T3Framework.Runtime.Input
{
	public class InputManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private InputActionAsset inputAsset;

		public static InputManager Instance { get; private set; }

		// Private

		// Static

		// Defined Functions
		public void Register(string actionMapName, string actionName, Action<InputAction.CallbackContext> callback)
		{
			RegisterStarted(actionMapName, actionName, callback);
		}

		public void RegisterStarted(string actionMapName, string actionName,
			Action<InputAction.CallbackContext> callback)
		{
			var actionMap = inputAsset.FindActionMap(actionMapName, true);
			var action = actionMap.FindAction(actionName, true);
			action.started += Encapsulate(callback);
		}

		public void RegisterPerformed(string actionMapName, string actionName,
			Action<InputAction.CallbackContext> callback)
		{
			var actionMap = inputAsset.FindActionMap(actionMapName, true);
			var action = actionMap.FindAction(actionName, true);
			action.performed += Encapsulate(callback);
		}

		public void RegisterCanceled(string actionMapName, string actionName,
			Action<InputAction.CallbackContext> callback)
		{
			var actionMap = inputAsset.FindActionMap(actionMapName, true);
			var action = actionMap.FindAction(actionName, true);
			action.canceled += Encapsulate(callback);
		}

		private Action<InputAction.CallbackContext> Encapsulate(Action<InputAction.CallbackContext> callback)
		{
			return context =>
			{
				if (!IsFocusingOnTextField()) callback(context);
			};
		}

		private static bool IsFocusingOnTextField()
		{
			GameObject selected = EventSystem.current.currentSelectedGameObject;
			if (selected == null) return false;
			InputField inputField = selected.GetComponent<InputField>();
			TMP_InputField inputField2 = selected.GetComponent<TMP_InputField>();
			return (inputField != null && inputField.isFocused) || (inputField2 != null && inputField2.isFocused);
		}

		// System Functions
		public void OnEnable()
		{
			Instance = this;
			inputAsset.Enable();
		}

		public void OnDisable()
		{
			inputAsset.Disable();
		}
	}
}