#nullable enable

using T3Framework.Runtime.I18N;
using UnityEngine;
using Yarn.Unity;

namespace MusicGame.EditorTutorial
{
	public class TutorialLocalizationAdapter : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private BuiltinLocalisedLineProvider lineProvider = default!;

		private void OnEnable()
		{
			I18NSystem.OnLanguageChanged += OnLanguageChanged;
			lineProvider.LocaleCode = I18NSystem.CurrentLanguageCode.GetAbbreviation();
		}

		private void OnDisable()
		{
			I18NSystem.OnLanguageChanged -= OnLanguageChanged;
		}

		private void OnLanguageChanged(LanguagePack pack)
		{
			lineProvider.LocaleCode = pack.LanguageCode.GetAbbreviation();
			lineProvider.AssetLocaleCode = pack.LanguageCode.GetAbbreviation();
		}

		[YarnCommand("language")]
		public static void ChangeLanguage(string languageCode)
		{
			var language = LanguageExtension.GetLanguage(languageCode);
			if (language == null) return;
			I18NSystem.CurrentLanguageCode = language.Value;
		}
	}
}