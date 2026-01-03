#nullable enable

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using T3Framework.Runtime;

namespace T3Framework.Static.Movement
{
	public interface IMoveList<T> : IEnumerable<KeyValuePair<T3Time, T>>
	{
		public int Count { get; }

		public bool Insert(T3Time time, T item);

		public bool Remove(T3Time time);

		public bool Contains(T item);

		public bool TryGet(T3Time time, [NotNullWhen(true)] out T? item);

		public Dictionary<T3Time, T> ToDictionary() => new(this);
	}
}