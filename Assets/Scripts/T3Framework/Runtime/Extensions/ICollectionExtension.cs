using System;
using System.Collections.Generic;

namespace T3Framework.Runtime.Extensions
{
	public static class ICollectionExtension
	{
		public static void AddIf<T>(this ICollection<T> collection, T item, bool predicate)
		{
			if (predicate)
			{
				collection.Add(item);
			}
		}
	}
}