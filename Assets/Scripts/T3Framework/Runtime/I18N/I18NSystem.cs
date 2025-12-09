// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using T3Framework.Runtime.Localization;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	// TODO: Merge T3Framework.Runtime.Localization here with runtime localization
	public static class I18NSystem
	{
		public const string DefaultLanguageCode = "zh-Hans";
		internal const string DefaultLanguageName = "SimplifiedChinese";
		private static readonly Dictionary<string, LanguagePack> languageMap = new();

		private static readonly LanguagePack defaultLanguagePack;
		private static LanguagePack currentLanguagePack;

		public static Dictionary<string, LanguagePack>.ValueCollection Languages => languageMap.Values;

		public static LanguagePack CurrentLanguage
		{
			get => currentLanguagePack;
			set
			{
				if (currentLanguagePack == value) return;

				if (!languageMap.ContainsKey(value.LanguageCode))
				{
					Debug.LogError("Language pack is not found in the dictionary.");
				}

				currentLanguagePack = value;
				OnLanguageChanged?.Invoke(value);
			}
		}

		public static event Action<LanguagePack>? OnLanguageChanged;

		static I18NSystem()
		{
			var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
			var files = Directory.GetFiles(folder);
			foreach (var file in files)
			{
				if (file.EndsWith(".yaml") || file.EndsWith(".yml"))
				{
					var fileName = Path.GetFileNameWithoutExtension(file);
					var language = LanguageExtension.GetLanguage(fileName);
					if (language is null) continue;
					if (LanguagePack.TryLoad(language.Value, file, out var pack))
					{
						languageMap.TryAdd(pack.LanguageCode, pack);
					}
				}
				else if (file.EndsWith(".txt"))
				{
					if (LanguagePack.TryLoadDeenoteStyle(file, out var pack))
					{
						languageMap.TryAdd(pack.LanguageCode, pack);
					}
				}
			}

			if (!languageMap.ContainsKey(DefaultLanguageCode))
			{
				Debug.LogWarning("Default language translation file is not found.");
				languageMap.Add(DefaultLanguageCode, LanguagePack.FallbackDefault);
			}

			currentLanguagePack = defaultLanguagePack = languageMap[DefaultLanguageCode];
		}

		public static string GetText(I18NText text) =>
			!text.IsLocalized
				? text.TextOrKey
				: currentLanguagePack.GetTranslationOrDefault(text.TextOrKey) ??
				  defaultLanguagePack.GetTranslationOrDefault(text.TextOrKey) ??
				  text.TextOrKey;

		public static string GetText(string key, params string[] args)
		{
			var text = currentLanguagePack.GetTranslationOrDefault(key) ??
			           defaultLanguagePack.GetTranslationOrDefault(key) ??
			           key;
			if (args.Length > 0)
			{
				var sb = new StringBuilder(text);
				for (int i = 0; i < args.Length; i++)
				{
					sb.Replace($"{{{i}}}", args[i]);
				}

				text = sb.ToString();
			}

			return text;
		}

		public static bool TrySetLanguage(string? languageCode)
		{
			if (languageCode is null) return false;

			if (languageMap.TryGetValue(languageCode, out var pack))
			{
				currentLanguagePack = pack;
				return true;
			}

			return false;
		}

		public static bool TryGetLanguagePack(string languageCode, out LanguagePack languagePack) =>
			languageMap.TryGetValue(languageCode, out languagePack);
	}
}