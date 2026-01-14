#nullable enable

using System;
using TMPro;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	public class I18NTextBlock : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private TextMeshProUGUI text = default!;
		[SerializeField] private string key = string.Empty;
		[SerializeField] private string[] args = Array.Empty<string>();

		// Defined Functions
		public void SetText(string key, params string[] args)
		{
			this.key = key;
			this.args = args;
			text.text = I18NSystem.GetText(key, args);
		}

		// Event Handlers
		private void OnLanguageChanged(LanguagePack pack) => text.text = I18NSystem.GetText(key, args);

		// System Functions
		void OnEnable()
		{
			text.text = I18NSystem.GetText(key, args);
			I18NSystem.OnLanguageChanged += OnLanguageChanged;
		}

		void OnDisable()
		{
			I18NSystem.OnLanguageChanged -= OnLanguageChanged;
		}
	}
}