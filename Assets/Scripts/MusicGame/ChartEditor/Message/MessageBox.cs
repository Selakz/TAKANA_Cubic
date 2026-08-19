#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.I18N;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Message
{
	public class MessageBox : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject root = default!;
		[SerializeField] private TMP_Text messageText = default!;
		[SerializeField] private Transform buttonParent = default!;
		[SerializeField] private GameObject buttonPrefab = default!;

		// Private
		private readonly List<Button> buttons = new();
		private Action<int>? onButtonClicked;

		// Defined Functions
		public void Show(
			string text, string[] buttonTexts, Action<int>? onButtonClicked, bool isKey = true, params string[] args)
		{
			if (buttonTexts.Length == 0)
			{
				Debug.LogWarning("MessageBox: buttonTexts should not be empty");
				return;
			}

			this.onButtonClicked = onButtonClicked;
			messageText.text = isKey ? I18NSystem.GetText(text, args) : text;

			for (var i = 0; i < buttonTexts.Length; i++)
			{
				var button = GetOrCreateButton(i);
				var label = button.GetComponentInChildren<TMP_Text>();
				label.text = isKey ? I18NSystem.GetText(buttonTexts[i]) : buttonTexts[i];
			}

			for (var i = buttonTexts.Length; i < buttons.Count; i++)
			{
				buttons[i].gameObject.SetActive(false);
			}

			root.SetActive(true);
			EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
		}

		public void Hide()
		{
			root.SetActive(false);
			onButtonClicked = null;
		}

		public void ShowConfirm(
			string text, Action? onButtonClicked, bool isKey = true, params string[] args)
		{
			var buttonTexts = isKey
				? new[] { "App_Confirm" }
				: new[] { I18NSystem.GetText("App_Confirm") };
			Show(text, buttonTexts, _ => onButtonClicked?.Invoke(), isKey, args);
		}

		public void ShowConfirmAndCancel(
			string text, Action<int>? onButtonClicked, bool isKey = true, params string[] args)
		{
			var buttonTexts = isKey
				? new[] { "App_Confirm", "App_Cancel" }
				: new[] { I18NSystem.GetText("App_Confirm"), I18NSystem.GetText("App_Cancel") };
			Show(text, buttonTexts, onButtonClicked, isKey, args);
		}

		private Button GetOrCreateButton(int index)
		{
			if (index < buttons.Count)
			{
				buttons[index].gameObject.SetActive(true);
				return buttons[index];
			}

			var buttonObject = Instantiate(buttonPrefab, buttonParent);
			var button = buttonObject.GetComponent<Button>();
			var buttonIndex = index;
			button.onClick.AddListener(() => OnButtonClicked(buttonIndex));
			buttons.Add(button);
			return button;
		}

		// Event Handlers
		private void OnButtonClicked(int index)
		{
			root.SetActive(false);
			onButtonClicked?.Invoke(index);
			onButtonClicked = null;
		}
	}
}