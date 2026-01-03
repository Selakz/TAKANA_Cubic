#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace T3Framework.Static.Collections.Generic
{
	public class HashSetMap<TKey, TValue>
	{
		private readonly SortedList<TKey, HashSet<TValue>> dictionary = new();

		public IEnumerable<TValue> this[TKey key]
		{
			get => dictionary.TryGetValue(key, out var list) ? list : Enumerable.Empty<TValue>();
			set => dictionary[key] = value.ToHashSet();
		}

		public IReadOnlyDictionary<TKey, HashSet<TValue>> Value => dictionary;

		public IList<TKey> Keys => dictionary.Keys;

		public IEnumerable<TValue> Values => dictionary.Values.SelectMany(v => v);

		public int Count => dictionary.Count;

		public HashSetMap()
		{
		}

		public HashSetMap(Comparer<TKey> comparer) => dictionary = new SortedList<TKey, HashSet<TValue>>(comparer);

		public void Add(TKey key, TValue value)
		{
			if (!dictionary.ContainsKey(key)) dictionary[key] = new();
			dictionary[key].Add(value);
		}

		public bool Remove(TKey key) => dictionary.Remove(key);

		public bool Remove(TKey key, TValue value)
		{
			if (dictionary.TryGetValue(key, out var list))
			{
				if (!list.Remove(value)) return false;
				if (list.Count == 0) dictionary.Remove(key);
				return true;
			}

			return false;
		}

		public void Clear() => dictionary.Clear();

		public bool ContainsKey(TKey key) => dictionary.ContainsKey(key);

		public bool TryGetValue(TKey key, out HashSet<TValue> value) => dictionary.TryGetValue(key, out value);
	}
}