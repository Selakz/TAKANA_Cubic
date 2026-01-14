// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	// TODO: Merge T3Framework.Runtime.Localization here with runtime localization
	public static class I18NSystem
	{
		public const Language DefaultLanguageCode = Language.SimplifiedChinese;
		private static readonly Dictionary<Language, LanguagePack> languageMap = new();

		private static LanguagePack defaultLanguagePack;
		private static LanguagePack currentLanguagePack;

		public static Dictionary<Language, LanguagePack>.ValueCollection Languages => languageMap.Values;

		public static Language CurrentLanguageCode
		{
			get => currentLanguagePack.LanguageCode;
			set
			{
				if (currentLanguagePack.LanguageCode == value) return;
				if (!languageMap.TryGetValue(value, out var pack))
				{
					Debug.LogError("Language pack is not found in the dictionary.");
					return;
				}

				CurrentLanguage = pack;
			}
		}

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

		static I18NSystem() => currentLanguagePack = defaultLanguagePack = LanguagePack.FallbackDefault;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void ConstructTexts() => ConstructTextsAsync().Forget();

		private static async UniTaskVoid ConstructTextsAsync()
		{
			var folder = Path.Combine(Application.streamingAssetsPath, "Languages");
			var files = FileHelper.GetFiles(folder);

			foreach (var file in files)
			{
				if (file.EndsWith(".yaml") || file.EndsWith(".yml"))
				{
					var fileName = Path.GetFileNameWithoutExtension(file);
					var language = LanguageExtension.GetLanguage(fileName);
					if (language is null) continue;
					var text = await FileHelper.ReadTextAsync(file).AsUniTask();
					if (text is null) continue;
					if (LanguagePack.TryLoad(language.Value, text, out var pack))
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
			OnLanguageChanged?.Invoke(currentLanguagePack);
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

		public static bool TrySetLanguage(Language languageCode)
		{
			if (languageMap.TryGetValue(languageCode, out var pack))
			{
				currentLanguagePack = pack;
				return true;
			}

			return false;
		}

		public static bool TryGetLanguagePack(Language languageCode, out LanguagePack languagePack) =>
			languageMap.TryGetValue(languageCode, out languagePack);
	}
}