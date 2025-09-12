#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;

namespace T3Framework.Runtime.Localization
{
	public class I18NString : IDictionary<Language, string>
	{
		private readonly Dictionary<Language, string> dict;

		public I18NString()
		{
			dict = new Dictionary<Language, string>();
			Clear();
		}

		public IEnumerator<KeyValuePair<Language, string>> GetEnumerator()
		{
			return dict.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<Language, string> item)
		{
			dict[item.Key] = item.Value;
		}

		/// <summary> Empty all contents, but the count does not change </summary>
		public void Clear()
		{
			foreach (Language language in Enum.GetValues(typeof(Language)))
			{
				dict[language] = string.Empty;
			}
		}

		public bool Contains(KeyValuePair<Language, string> item)
		{
			return dict.ContainsKey(item.Key) && !string.IsNullOrEmpty(dict[item.Key]);
		}

		public void CopyTo(KeyValuePair<Language, string>[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public bool Remove(KeyValuePair<Language, string> item)
		{
			dict[item.Key] = string.Empty;
			return true;
		}

		public int Count => dict.Count;
		public bool IsReadOnly => false;

		public void Add(Language key, string value)
		{
			dict[key] = value;
		}

		public bool ContainsKey(Language key)
		{
			return dict.ContainsKey(key) && !string.IsNullOrEmpty(dict[key]);
		}

		public bool Remove(Language key)
		{
			dict[key] = string.Empty;
			return true;
		}

		public bool TryGetValue(Language key, out string value)
		{
			return dict.TryGetValue(key, out value);
		}

		public string this[Language key]
		{
			get => dict[key];
			set => dict[key] = value;
		}

		public ICollection<Language> Keys => dict.Keys;
		public ICollection<string> Values => dict.Values;
	}
}