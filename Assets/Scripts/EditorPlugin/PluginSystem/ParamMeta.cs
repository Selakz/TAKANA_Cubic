#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UnityEngine;

namespace EditorPlugin.PluginSystem
{
	public class OptionsMeta : IParamMeta
	{
		public readonly struct Option
		{
			public string Text { get; }

			public object Value { get; }

			public Option(string text, object value)
			{
				Text = text;
				Value = value;
			}
		}

		public IReadOnlyList<Option> Options { get; }

		public OptionsMeta(IReadOnlyList<Option> options)
		{
			Options = options;
		}
	}

	public class ClampMeta : IParamMeta
	{
		public double? Min { get; }

		public double? Max { get; }

		public ClampMeta(double? min, double? max)
		{
			Min = min;
			Max = max;
		}
	}

	public class TimingMeta : IParamMeta
	{
	}

	public class FormatMeta : IParamMeta
	{
		public string Format { get; }

		public FormatMeta(string format)
		{
			Format = format;
		}
	}

	public static class ParamMetaParser
	{
		public static IReadOnlyList<IParamMeta> Parse(string? meta, Type type)
		{
			var result = new List<IParamMeta>();
			if (string.IsNullOrWhiteSpace(meta)) return result;

			foreach (var segment in SplitTopLevel(meta!, ','))
			{
				string trimmed = segment.Trim();
				if (trimmed.Length == 0) continue;

				var (name, args) = SplitNameAndArgs(trimmed);
				switch (name)
				{
					case "options":
						if (TryParseOptions(args, type, out var optionsMeta)) result.Add(optionsMeta);
						break;
					case "clamp":
						if (TryParseClamp(args, type, out var clampMeta)) result.Add(clampMeta);
						break;
					case "timing":
						if (type == typeof(int)) result.Add(new TimingMeta());
						else WarnNotApplicable("timing", type);
						break;
					case "format":
						if (IsNumericType(type)) result.Add(new FormatMeta(args));
						else WarnNotApplicable("format", type);
						break;
					default:
						Debug.LogWarning($"Unknown plugin param meta: {name}");
						break;
				}
			}

			return result;
		}

		// Setting aside lexical analysis, deepseek says the following handwritten parsing is the best solution and I think so!
		private static bool TryParseOptions(string args, Type type, [NotNullWhen(true)] out OptionsMeta? meta)
		{
			meta = null;
			if (!IsOptionsType(type))
			{
				WarnNotApplicable("options", type);
				return false;
			}

			var options = new List<OptionsMeta.Option>();
			foreach (var item in SplitArgs(args))
			{
				string trimmed = item.Trim();
				if (trimmed.Length == 0) continue;
				if (!TrySplitOption(trimmed, out var text, out var valueToken)) return false;
				if (!TryParseValue(valueToken, type, out var value)) return false;
				options.Add(new OptionsMeta.Option(text ?? valueToken, value));
			}

			if (options.Count == 0)
			{
				Debug.LogWarning("Plugin param meta 'options' has no valid options.");
				return false;
			}

			meta = new OptionsMeta(options);
			return true;
		}

		private static bool TryParseClamp(string args, Type type, [NotNullWhen(true)] out ClampMeta? meta)
		{
			meta = null;
			if (!IsNumericType(type))
			{
				WarnNotApplicable("clamp", type);
				return false;
			}

			var parts = new List<string>();
			foreach (var part in SplitArgs(args))
			{
				string trimmed = part.Trim();
				if (trimmed.Length > 0) parts.Add(trimmed);
			}

			if (parts.Count != 2)
			{
				Debug.LogWarning($"Plugin param meta 'clamp' expects 2 bounds, got {parts.Count}: {args}");
				return false;
			}

			if (!TryParseBound(parts[0], out var min) || !TryParseBound(parts[1], out var max)) return false;
			meta = new ClampMeta(min, max);
			return true;
		}

		private static bool TryParseBound(string token, out double? bound)
		{
			if (token == "_")
			{
				bound = null;
				return true;
			}

			if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
			{
				bound = value;
				return true;
			}

			Debug.LogWarning($"Cannot parse clamp bound: {token}");
			bound = null;
			return false;
		}

		private static bool TrySplitOption(string s, out string? text, out string valueToken)
		{
			text = null;
			valueToken = string.Empty;

			if (s.StartsWith('\''))
			{
				int end = s.IndexOf('\'', 1);
				if (end < 0)
				{
					Debug.LogWarning($"Unclosed quote in plugin param meta option: {s}");
					return false;
				}

				text = s[1..end];
				string rest = s[(end + 1)..].Trim();
				if (rest.StartsWith(':')) rest = rest[1..].Trim();
				else if (rest.Length > 0)
				{
					Debug.LogWarning($"Unexpected content after quoted option text: {s}");
					return false;
				}

				valueToken = rest;
				return true;
			}

			int colon = s.IndexOf(':');
			if (colon >= 0)
			{
				text = s[..colon].Trim();
				valueToken = s[(colon + 1)..].Trim();
			}
			else
			{
				valueToken = s.Trim();
			}

			return true;
		}

		private static bool TryParseValue(string token, Type type, out object value)
		{
			value = token;
			if (type == typeof(int))
			{
				if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
				{
					value = intValue;
					return true;
				}
			}
			else if (type == typeof(float))
			{
				if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatValue))
				{
					value = floatValue;
					return true;
				}
			}
			else if (type == typeof(string))
			{
				return true;
			}

			Debug.LogWarning($"Cannot parse value '{token}' as {type.Name} for plugin param meta.");
			return false;
		}

		private static (string Name, string Args) SplitNameAndArgs(string segment)
		{
			int start = segment.IndexOf('(');
			if (start < 0) return (segment.Trim(), string.Empty);

			int end = FindClosingParen(segment, start);
			if (end < 0)
			{
				Debug.LogWarning($"Unclosed parenthesis in plugin param meta: {segment}");
				return (segment.Trim(), string.Empty);
			}

			return (segment[..start].Trim(), segment[(start + 1)..end].Trim());
		}

		private static int FindClosingParen(string text, int openIndex)
		{
			bool inQuote = false;
			int depth = 0;
			for (int i = openIndex; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '\'') inQuote = !inQuote;
				else if (c == '(' && !inQuote) depth++;
				else if (c == ')' && !inQuote)
				{
					depth--;
					if (depth == 0) return i;
				}
			}

			return -1;
		}

		private static IEnumerable<string> SplitTopLevel(string text, char separator)
		{
			var parts = new List<string>();
			bool inQuote = false;
			int depth = 0;
			int start = 0;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '\'') inQuote = !inQuote;
				else if (c == '(' && !inQuote) depth++;
				else if (c == ')' && !inQuote) depth--;
				else if (c == separator && depth == 0 && !inQuote)
				{
					parts.Add(text[start..i]);
					start = i + 1;
				}
			}

			parts.Add(text[start..]);
			return parts;
		}

		private static IEnumerable<string> SplitArgs(string args)
		{
			var parts = new List<string>();
			bool inQuote = false;
			int start = 0;
			for (int i = 0; i < args.Length; i++)
			{
				char c = args[i];
				if (c == '\'') inQuote = !inQuote;
				else if (c == ',' && !inQuote)
				{
					parts.Add(args[start..i]);
					start = i + 1;
				}
			}

			parts.Add(args[start..]);
			return parts;
		}

		private static bool IsOptionsType(Type type) =>
			type == typeof(string) || type == typeof(int) || type == typeof(float);

		private static bool IsNumericType(Type type) => type == typeof(int) || type == typeof(float);

		private static void WarnNotApplicable(string metaName, Type type)
		{
			Debug.LogWarning($"Plugin param meta '{metaName}' is not applicable to type {type.Name}, ignored.");
		}
	}
}