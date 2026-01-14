// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace T3Framework.Runtime.I18N
{
	public sealed class LanguagePack
	{
		private readonly Dictionary<string, string> translations;

		public Language LanguageCode { get; }

		public string this[string key] => translations[key];

		public string? GetTranslationOrDefault(string key) => translations.GetValueOrDefault(key);

		private LanguagePack(Language code, Dictionary<string, string> translations)
		{
			LanguageCode = code;
			this.translations = translations;
		}

		public static bool TryLoad(Language language, string yamlContent, [NotNullWhen(true)] out LanguagePack? pack)
		{
			pack = null;
			var map = new Dictionary<string, string>();

			try
			{
				var yamlStream = new YamlStream();
				yamlStream.Load(new StringReader(yamlContent));

				var root = yamlStream.Documents[0].RootNode;
				if (root is YamlMappingNode mappingNode)
				{
					FlattenMapping(mappingNode, "", map);
				}
				else return false;
			}
			catch
			{
				return false;
			}

			pack = new LanguagePack(language, map);
			return true;
		}

		private static void FlattenMapping(YamlMappingNode node, string prefix, Dictionary<string, string> map)
		{
			foreach (var (yamlNode, valueNode) in node.Children)
			{
				if (yamlNode is not YamlScalarNode keyNode) continue;

				string key = keyNode.Value!;
				string fullKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}_{key}";

				switch (valueNode)
				{
					case YamlScalarNode scalarNode:
						map[fullKey] = scalarNode.Value ?? string.Empty;
						break;
					case YamlMappingNode subMapping:
						FlattenMapping(subMapping, fullKey, map);
						break;
					default:
						continue;
				}
			}
		}

		private static LanguagePack? fallbackDefault;

		internal static LanguagePack FallbackDefault => fallbackDefault ??= new LanguagePack(
			I18NSystem.DefaultLanguageCode,
			new Dictionary<string, string>());
	}
}