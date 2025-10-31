using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Runtime.Extensions
{
	public static class UnityExtension
	{
		// Color
		public static string ToHexAlphaTuple(this Color color)
		{
			int r = Mathf.RoundToInt(color.r * 255);
			int g = Mathf.RoundToInt(color.g * 255);
			int b = Mathf.RoundToInt(color.b * 255);
			return $"({r:X2}{g:X2}{b:X2}, {color.a:0.00})";
		}

		public static Color Inverse(this Color color, bool inverseAlpha = false)
		{
			float r = 1 - color.r;
			float g = 1 - color.g;
			float b = 1 - color.b;
			float a = inverseAlpha ? 1 - color.a : color.a;
			return new Color(r, g, b, a);
		}

		// Dropdown
		public static T[] SetOptions<T>(
			this TMP_Dropdown dropdown, List<T> options, Func<T, string> optionTextConverter)
		{
			List<TMP_Dropdown.OptionData> optionTexts =
				options.Select(option => new TMP_Dropdown.OptionData(optionTextConverter(option))).ToList();
			dropdown.options = optionTexts;
			return options.ToArray();
		}

		public static T[] SetOptions<T>(
			this Dropdown dropdown, List<T> options, Func<T, string> optionTextConverter)
		{
			List<Dropdown.OptionData> optionTexts =
				options.Select(option => new Dropdown.OptionData(optionTextConverter(option))).ToList();
			dropdown.options = optionTexts;
			return options.ToArray();
		}
	}
}