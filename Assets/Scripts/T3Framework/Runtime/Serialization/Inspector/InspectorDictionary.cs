#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace T3Framework.Runtime.Serialization.Inspector
{
	[Serializable]
	public struct KeyValue<TKey, TValue>
	{
		[SerializeField] private TKey key;
		[SerializeField] private TValue value;

		public TKey Key => key;

		public TValue Value => value;

		public KeyValuePair<TKey, TValue> KeyValuePair => new(key, value);
	}

	[Serializable]
	public class InspectorDictionary<TKey, TValue>
	{
		[SerializeField] protected List<KeyValue<TKey, TValue>> pairs = new();

		private Dictionary<TKey, TValue>? dictionary;

		public Dictionary<TKey, TValue> Value => dictionary ??=
			new Dictionary<TKey, TValue>(pairs.Select(pair => pair.KeyValuePair));

		public void SortAscendByKey()
		{
			pairs.Sort((a, b) => Comparer<TKey>.Default.Compare(a.Key, b.Key));
		}

		public void SortDescendByKey()
		{
			pairs.Sort((a, b) => Comparer<TKey>.Default.Compare(b.Key, a.Key));
		}

		public void SortAscendByValue()
		{
			pairs.Sort((a, b) => Comparer<TValue>.Default.Compare(a.Value, b.Value));
		}

		public void SortDescendByValue()
		{
			pairs.Sort((a, b) => Comparer<TValue>.Default.Compare(b.Value, a.Value));
		}
	}
}