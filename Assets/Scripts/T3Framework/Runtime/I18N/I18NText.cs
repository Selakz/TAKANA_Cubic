// Modified from Deenote: https://github.com/Chlorie/Deenote

#nullable enable

using System;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
#endif

namespace T3Framework.Runtime.I18N
{
	[Serializable]
	public struct I18NText : IEquatable<I18NText>
	{
		[SerializeField] private bool isLocalized;
		[SerializeField] private string textOrKey;
		public readonly bool IsLocalized => isLocalized;
		public readonly string TextOrKey => textOrKey;

		private I18NText(bool isLocalized, string textOrKey)
		{
			this.isLocalized = isLocalized;
			this.textOrKey = textOrKey;
		}

		public static I18NText Raw(string text) => new(false, text);

		public static I18NText Localized(string textKey) => new(true, textKey);

		public readonly override bool Equals(object? obj) => obj is I18NText text && this == text;
		public readonly override int GetHashCode() => HashCode.Combine(IsLocalized, TextOrKey);

		public static bool operator ==(I18NText left, I18NText right) =>
			left.IsLocalized == right.IsLocalized && left.TextOrKey == right.TextOrKey;

		public static bool operator !=(I18NText left, I18NText right) => !(left == right);

		public static bool operator ==(I18NText left, string right) => left == Raw(right);
		public static bool operator !=(I18NText left, string right) => left != Raw(right);

		public bool Equals(I18NText other) => isLocalized == other.isLocalized && textOrKey == other.textOrKey;
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(I18NText))]
	public class LocalizableTextPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var isLocalizedProp = property.FindPropertyRelative("isLocalized");
			var textOrKeyProp = property.FindPropertyRelative("textOrKey");

			var propertyNameLabel = new Label(NormalizePropertyName(property.name)) { style = { minWidth = 122.5f } };
			var isLocalizedToggle = new Toggle { bindingPath = isLocalizedProp.propertyPath };
			var textOrKeyLabel = new Label { style = { width = 30f, marginLeft = 5f } };
			var textOrKeyInput = new TextField { bindingPath = textOrKeyProp.propertyPath, style = { flexGrow = 1 } };

			isLocalizedToggle.RegisterValueChangedCallback(ev => textOrKeyLabel.text = ev.newValue ? "Key" : "Text");

			var valueContainer = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
			valueContainer.Add(isLocalizedToggle);
			valueContainer.Add(textOrKeyLabel);
			valueContainer.Add(textOrKeyInput);

			var horizontalContainer = new VisualElement { style = { flexDirection = FlexDirection.Row } };
			horizontalContainer.Add(propertyNameLabel);
			horizontalContainer.Add(valueContainer);

			return horizontalContainer;
		}

		private static string NormalizePropertyName(ReadOnlySpan<char> propertyName)
		{
			var start = 0;
			while (propertyName[start] == '_')
				start++;

			propertyName = propertyName[start..];
			Span<bool> seperates = stackalloc bool[propertyName.Length];
			int spaceCount = 0;
			for (int i = 1; i < propertyName.Length - 1; i++)
			{
				if (char.IsUpper(propertyName[i]) && char.IsLower(propertyName[i + 1]))
				{
					seperates[i] = true;
					spaceCount++;
				}
			}

			Span<char> chars = stackalloc char[seperates.Length + spaceCount];
			chars[0] = char.ToUpper(propertyName[0]);
			var index = 1;
			for (int i = 1; i < propertyName.Length; i++)
			{
				if (seperates[i])
					chars[index++] = ' ';
				chars[index++] = propertyName[i];
			}

			return chars.ToString();
		}
	}
#endif
}