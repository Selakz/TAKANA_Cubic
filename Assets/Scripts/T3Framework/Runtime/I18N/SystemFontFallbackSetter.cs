// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	public class SystemFontFallbackSetter : MonoBehaviour
	{
		[SerializeField] private TMP_FontAsset fontAsset = default!;
		[SerializeField] private SystemFontFallbackSettings settings = default!;

		private readonly Dictionary<string, TMP_FontAsset> systemFontAssets = new();
		private string[] installedFontPaths = null!;

		private void Awake()
		{
			installedFontPaths = Font.GetPathsToOSFonts();
			I18NSystem.OnLanguageChanged += LanguageChanged;
			LanguageChanged(I18NSystem.CurrentLanguage);
		}

		private void LanguageChanged(LanguagePack languagePack)
		{
			var languageCode = languagePack.LanguageCode;
			if (settings.Settings.FirstOrDefault(s => s.languageCode == languageCode)
				    is var setting && setting.languageCode != languageCode)
				return;
			SetFontFallbacks(setting.fontFileNames);
		}

		private void SetFontFallbacks(string[] fontFileName)
		{
			var lastResort = fontAsset.fallbackFontAssetTable[^1];
			var table = fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
			foreach (var fileName in fontFileName)
			{
				if (systemFontAssets.TryGetValue(fileName, out var asset))
				{
					table.Add(asset);
					continue;
				}

				if (installedFontPaths.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p) == fileName)
				    is { } path)
					table.Add(systemFontAssets[fileName] = TMP_FontAsset.CreateFontAsset(new Font(path)));
			}

			table.Add(lastResort);
		}
	}
}