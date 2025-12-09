// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using T3Framework.Runtime.Localization;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace T3Framework.Runtime.I18N
{
	public sealed class LanguagePack
	{
		private readonly Dictionary<string, string> translations;

		public string LanguageCode { get; }

		public string LanguageDisplayName { get; }

		public string this[string key] => translations[key];

		public string? GetTranslationOrDefault(string key) => translations.GetValueOrDefault(key);

		private LanguagePack(string code, string displayName, Dictionary<string, string> translations)
		{
			LanguageCode = code;
			LanguageDisplayName = displayName;
			this.translations = translations;
		}

		internal static bool TryLoadDeenoteStyle(string filePath, [NotNullWhen(true)] out LanguagePack? pack)
		{
			using var reader = File.OpenText(filePath);
			pack = null;
			var code = reader.ReadLine();
			if (code is null) return false;

			var name = reader.ReadLine();
			if (name is null) return false;

			var translations = new Dictionary<string, string>();

			while (reader.ReadLine() is { } line)
			{
				if (line.StartsWith('#') || // Ignore comments
				    line.IndexOf('=') is var separator && separator < 0) // Ignore lines without '='
					continue;

				ReadOnlySpan<char> firstLineValueSpan = line.AsSpan(separator + 1);
				var value = firstLineValueSpan.SequenceEqual("\"\"\"")
					? ReadMultilineText()
					: firstLineValueSpan.ToString().Replace("<br/>", "\n"); // Multiline text

				string key = line[..separator];
				if (!translations.TryAdd(key, value))
					Debug.LogWarning($"Language pack {name} contains duplicated key: {key}");
			}

			pack = new LanguagePack(code, name, translations);
			return true;

			string ReadMultilineText()
			{
				var lines = new List<string>();
				int skipCount = 0;
				while (reader.ReadLine() is { } line)
				{
					if (line.EndsWith("\"\"\"") && line.AsSpan()[..^3].IsWhiteSpace())
					{
						skipCount = line.Length - 3;
						break;
					}

					lines.Add(line);
				}

				var sb = new StringBuilder();
				foreach (var line in lines)
				{
					int actualSkipCount = GetLeadingSpaceCount(line, skipCount);
					sb.AppendLine(line[actualSkipCount..]);
				}

				return sb.ToString();

				static int GetLeadingSpaceCount(string str, int max)
				{
					for (int i = 0; i < str.Length; i++)
					{
						if (i >= max || !char.IsWhiteSpace(str[i]))
							return i;
					}

					return str.Length;
				}
			}
		}

		public static bool TryLoad(Language language, string filePath, [NotNullWhen(true)] out LanguagePack? pack)
		{
			pack = null;
			var map = new Dictionary<string, string>();
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return false;

			try
			{
				string yamlContent = File.ReadAllText(filePath, Encoding.UTF8);
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

			pack = new LanguagePack(language.GetAbbreviation(), language.ToString(), map);
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
			I18NSystem.DefaultLanguageName,
			new Dictionary<string, string>());
	}
}