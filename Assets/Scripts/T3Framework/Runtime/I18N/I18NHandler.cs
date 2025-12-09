// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class I18NHandler : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private I18NText i18NText;

		// Private
		private List<string>? i18NArgs;

		private TextMeshProUGUI Text => tmpText ??= GetComponent<TextMeshProUGUI>();
		private TextMeshProUGUI? tmpText;

		// Defined Functions
		public void SetText(I18NText text, ReadOnlySpan<string> args = default)
		{
			bool valueChanged = false;
			if (i18NText != text)
			{
				i18NText = text;
				valueChanged = true;
			}

			var oldArgs = i18NArgs?.ToArray() ?? ReadOnlySpan<string>.Empty;
			if (!args.SequenceEqual(oldArgs))
			{
				if (i18NArgs is null) i18NArgs = new List<string>(args.Length);
				else i18NArgs.Clear();
				foreach (var arg in args) i18NArgs.Add(arg);
				valueChanged = true;
			}

			if (valueChanged) RefreshDisplayText();
		}

		public void SetText(ArgedI18NText text) => SetText(text.I18NText, text.Args);

		private void RefreshDisplayText()
		{
			string text = I18NSystem.GetText(i18NText);
			if (i18NArgs?.Count > 0)
			{
				var sb = new StringBuilder(text);
				for (int i = 0; i < i18NArgs.Count; i++)
				{
					sb.Replace($"{{{i}}}", i18NArgs[i]);
				}

				Text.text = sb.ToString();
			}
			else
			{
				Text.text = text;
			}
		}

		// Event Handlers
		private void OnLanguageChanged(LanguagePack pack) => RefreshDisplayText();

		// System Functions
		void OnEnable()
		{
			I18NSystem.OnLanguageChanged += OnLanguageChanged;
			OnLanguageChanged(I18NSystem.CurrentLanguage);
		}

		void OnDisable()
		{
			I18NSystem.OnLanguageChanged -= OnLanguageChanged;
		}
	}
}