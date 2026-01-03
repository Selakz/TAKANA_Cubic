#nullable enable

using System;
using System.Collections.Generic;

namespace T3Framework.Runtime.ECS
{
	public interface INotifyDataset<out T> : IEnumerable<T>
	{
		public event Action<T>? OnDataAdded; // For application logics
		public event Action<T>? OnDataAddedInherit; // For dataset variants to inherit the data change

		public event Action<T>? BeforeDataRemoved; // For application logics
		public event Action<T>? BeforeDataRemovedInherit; // For dataset variants to inherit the data change

		public event Action<T>? OnDataUpdated;
		// TODO: public event Action<T>? OnDataUpdatedInherit or better way
	}

	/// <summary> The dataset who update its data on itself, like listening to some events. </summary>
	public interface IReadOnlyDataset<T> : INotifyDataset<T>, IDisposable where T : IComponent
	{
		public int Count { get; }

		public bool Contains(T component);
	}

	public interface IDataset<T> : IReadOnlyDataset<T> where T : IComponent
	{
		public bool Add(T item);

		public bool Remove(T item);

		public void Clear();
	}
}