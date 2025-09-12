using System.Collections.Generic;

namespace T3Framework.Runtime.Extensions
{
	public static class IDictionaryExtension
	{
		public static void AddIf<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			TKey key, TValue value, bool predicate)
		{
			if (predicate)
			{
				dictionary.Add(key, value);
			}
		}

		public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
			TKey key, TValue defaultValue = default)
		{
			return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
		}
	}
}