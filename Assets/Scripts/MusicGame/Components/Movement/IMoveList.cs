using System.Collections.Generic;
using T3Framework.Runtime;

namespace MusicGame.Components.Movement
{
	public interface IMoveItem
	{
		public T3Time Time { get; }

		public IMoveItem SetTime(T3Time time);
	}

	public interface IMoveList : IEnumerable<IMoveItem>
	{
		public int Count { get; }

		public bool Insert(IMoveItem item);

		public bool Remove(T3Time time);

		public bool TryGet(T3Time time, out IMoveItem item);
	}

	public interface IMoveList<T> : IEnumerable<T> where T : IMoveItem
	{
		public int Count { get; }

		public bool Insert(T item);

		public bool Remove(T3Time time);

		public bool Contains(T item);

		public bool TryGet(T3Time time, out T item);
	}
}