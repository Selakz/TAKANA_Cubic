// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace T3Framework.Runtime.I18N
{
	public struct ArgedI18NText : IEquatable<ArgedI18NText>
	{
		private readonly I18NText i18NText;
		private object? args;

		public readonly I18NText I18NText => i18NText;

		public readonly bool IsLocalized => i18NText.IsLocalized;

		public readonly string TextOrKey => i18NText.TextOrKey;

		public readonly ReadOnlySpan<string> Args
		{
			get
			{
				switch (args)
				{
					case null:
						return ReadOnlySpan<string>.Empty;
					case string[] arr:
						return arr.AsSpan();
					default:
						Debug.Assert(args is string);
						return MemoryMarshal.CreateReadOnlySpan(
							ref Unsafe.As<object, string>(ref Unsafe.AsRef(in args)), 1);
				}
			}
		}

		private ArgedI18NText(I18NText text, object? args)
		{
			i18NText = text;
			this.args = args;
			Debug.Assert(args is null or string or string[]);
		}

		public static ArgedI18NText Raw(string text) => new(I18NText.Raw(text), null);
		public static ArgedI18NText Localized(string textKey) => new(I18NText.Localized(textKey), null);

		public static ArgedI18NText Localized(string textKey, string arg0) =>
			new(I18NText.Localized(textKey), arg0);

		public static ArgedI18NText Localized(string textKey, params string[] args) =>
			new(I18NText.Localized(textKey), args);

		public readonly override bool Equals(object? obj) => obj is ArgedI18NText text && this == text;
		public readonly override int GetHashCode() => HashCode.Combine(i18NText, args);

		public static implicit operator ArgedI18NText(I18NText text) => new(text, null);

		public static bool operator ==(ArgedI18NText left, ArgedI18NText right)
		{
			if (left.i18NText != right.i18NText)
				return false;

			if (left.args == right.args)
				return true;

			// Deep equal
			return (left.args, right.args) switch
			{
				(string l, string r) => l == r,
				(string[] { Length: 1 } l, string r) => l[0] == r,
				(string l, string[] { Length: 1 } r) => l == r[0],
				(string[] l, string[] r) => l.AsSpan().SequenceEqual(r),
				_ => false
			};
		}

		public static bool operator !=(ArgedI18NText left, ArgedI18NText right) => !(left == right);

		public static bool operator ==(ArgedI18NText left, string right) => left == Raw(right);
		public static bool operator !=(ArgedI18NText left, string right) => left != Raw(right);

		public static bool operator ==(ArgedI18NText left, I18NText right) =>
			left == new ArgedI18NText(right, null);

		public static bool operator !=(ArgedI18NText left, I18NText right) =>
			left != new ArgedI18NText(right, null);

		public bool Equals(ArgedI18NText other) =>
			i18NText.Equals(other.i18NText) && Equals(args, other.args);
	}
}