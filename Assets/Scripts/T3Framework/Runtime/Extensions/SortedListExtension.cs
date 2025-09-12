using System.Collections.Generic;

namespace T3Framework.Runtime.Extensions
{
	public static class SortedListExtension
	{
		public static int BinarySearch<TKey, TValue>(this SortedList<TKey, TValue> sortedList, TKey key)
		{
			var keys = sortedList.Keys;
			int left = 0, right = keys.Count - 1;
			var comparer = sortedList.Comparer;
			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				int cmp = comparer.Compare(keys[mid], key);
				switch (cmp)
				{
					case 0:
						return mid;
					case < 0:
						left = mid + 1;
						break;
					default:
						right = mid - 1;
						break;
				}
			}

			return ~left;
		}
	}
}