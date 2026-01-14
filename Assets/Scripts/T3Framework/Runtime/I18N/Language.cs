#nullable enable

using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.Extensions;

namespace T3Framework.Runtime.I18N
{
	public enum Language
	{
		English,
		SimplifiedChinese,
		TraditionalChinese,
		Japanese
	}

	public static class LanguageExtension
	{
		private static readonly Dictionary<Language, string> abbrDictionary = new()
		{
			[Language.English] = "en",
			[Language.SimplifiedChinese] = "zh-Hans",
			[Language.TraditionalChinese] = "zh-Hant",
			[Language.Japanese] = "ja"
		};

		public static string GetAbbreviation(this Language language)
		{
			return abbrDictionary.Get(language, string.Empty);
		}

		public static Language? GetLanguage(string abbreviation)
		{
			return (from pair in abbrDictionary where pair.Value == abbreviation select pair.Key).FirstOrDefault();
		}
	}
}