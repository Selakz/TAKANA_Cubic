using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace T3Framework.Runtime.Extensions
{
	public static class RegexHelper
	{
		private static readonly Dictionary<int, Regex> tupleCache = new()
		{
			{ 0, new Regex(@"^\(\s*\)$") },
			{ 1, new Regex(@"^\((.+?)\)$") },
			{ 2, new Regex(@"^\((.+?),\s*(.+?)\)$") },
			{ 3, new Regex(@"^\((.+?),\s*(.+?),\s*(.+?)\)$") },
			{ 4, new Regex(@"^\((.+?),\s*(.+?),\s*(.+?),\s*(.+?)\)$") },
		};

		public static Match MatchTuple(string input, int count)
		{
			if (string.IsNullOrEmpty(input))
			{
				throw new ArgumentException("Input string cannot be null or empty");
			}

			if (tupleCache.TryGetValue(count, out var value))
			{
				var match = value.Match(input);
				if (!match.Success)
				{
					throw new FormatException("Input string was not in a correct format.");
				}

				return match;
			}

			throw new NotSupportedException();
		}
	}

	public static class UnityParser
	{
		public static Quaternion ParseQuaternion(string s)
		{
			var match = RegexHelper.MatchTuple(s, 4);
			try
			{
				float x = float.Parse(match.Groups[1].Value);
				float y = float.Parse(match.Groups[2].Value);
				float z = float.Parse(match.Groups[3].Value);
				float w = float.Parse(match.Groups[4].Value);
				return new Quaternion(x, y, z, w);
			}
			catch (FormatException ex)
			{
				throw new FormatException("Failed to parse one or more quaternion components", ex);
			}
		}

		public static Vector3 ParseVector3(string s)
		{
			var match = RegexHelper.MatchTuple(s, 3);
			try
			{
				float x = float.Parse(match.Groups[1].Value);
				float y = float.Parse(match.Groups[2].Value);
				float z = float.Parse(match.Groups[3].Value);
				return new Vector3(x, y, z);
			}
			catch (FormatException ex)
			{
				throw new FormatException("Failed to parse one or more vector3 components", ex);
			}
		}

		/// <summary>
		/// Parse RRGGBB type string to color.
		/// </summary>
		public static Color ParseHexColor(string s)
		{
			byte r = byte.Parse(s[..2], System.Globalization.NumberStyles.HexNumber);
			byte g = byte.Parse(s[2..4], System.Globalization.NumberStyles.HexNumber);
			byte b = byte.Parse(s[4..6], System.Globalization.NumberStyles.HexNumber);
			return new Color(r / 255f, g / 255f, b / 255f);
		}

		public static Color ParseHexAlphaTuple(string s)
		{
			var match = RegexHelper.MatchTuple(s, 2);
			try
			{
				string hexColor = match.Groups[1].Value;
				float a = float.Parse(match.Groups[2].Value);
				Color color = ParseHexColor(hexColor);
				color.a = a;
				return color;
			}
			catch (FormatException ex)
			{
				throw new FormatException("Failed to parse one or more color components", ex);
			}
		}
	}
}