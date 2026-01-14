#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace T3Framework.Runtime.I18N
{
	public class I18NString : IDictionary<Language, string>
	{
		private readonly Dictionary<Language, string> dict = new();

		public IEnumerator<KeyValuePair<Language, string>> GetEnumerator() => dict.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Add(KeyValuePair<Language, string> item) => dict[item.Key] = item.Value;

		public void Clear() => dict.Clear();

		public bool Contains(KeyValuePair<Language, string> item) =>
			dict.ContainsKey(item.Key) && !string.IsNullOrEmpty(dict[item.Key]);

		public void CopyTo(KeyValuePair<Language, string>[] array, int arrayIndex) => throw new NotSupportedException();

		public bool Remove(KeyValuePair<Language, string> item) => dict.Remove(item.Key);

		public int Count => dict.Count;
		public bool IsReadOnly => false;

		public void Add(Language key, string value) => dict[key] = value;

		public bool ContainsKey(Language key) => dict.ContainsKey(key) && !string.IsNullOrEmpty(dict[key]);

		public bool Remove(Language key) => dict.Remove(key);

		public bool TryGetValue(Language key, out string value) => dict.TryGetValue(key, out value);

		public string Value => this[I18NSystem.CurrentLanguageCode];

		public string this[Language key]
		{
			get => dict.TryGetValue(key, out var value)
				? value
				: dict.TryGetValue(Language.English, out value)
					? value
					: string.Empty;
			set => Add(key, value);
		}

		public ICollection<Language> Keys => dict.Keys;
		public ICollection<string> Values => dict.Values;
	}
}