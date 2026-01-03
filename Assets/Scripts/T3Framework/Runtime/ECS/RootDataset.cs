#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace T3Framework.Runtime.ECS
{
	public abstract class RootDataset<T> : IDataset<T> where T : IComponent
	{
		public event Action<T>? OnDataAdded;
		public event Action<T>? OnDataAddedInherit;
		public event Action<T>? BeforeDataRemoved;
		public event Action<T>? BeforeDataRemovedInherit;
		public event Action<T>? OnDataUpdated;

		public abstract int Count { get; }

		public void UnionWith(IEnumerable<T> other)
		{
			foreach (var item in other) AddToDataset(item);
		}

		public bool Add(T item)
		{
			if (AddToDataset(item))
			{
				item.OnComponentUpdated += OnDataModelUpdated;
				OnDataAddedInherit?.Invoke(item);
				OnDataAdded?.Invoke(item);
				return true;
			}

			return false;
		}

		public bool Remove(T item)
		{
			if (IsRemovable(item))
			{
				item.OnComponentUpdated -= OnDataModelUpdated;
				BeforeDataRemoved?.Invoke(item);
				BeforeDataRemovedInherit?.Invoke(item);
				RemoveFromDataset(item);
				return true;
			}

			return false;
		}

		public void Clear()
		{
			var list = this.ToList();
			foreach (var item in list) Remove(item);
		}

		public abstract bool Contains(T item);

		private void OnDataModelUpdated(object itemObject, EventArgs _)
		{
			if (itemObject is not T item) return;
			if (NeedToRemove(item)) Remove(item);
			else OnDataUpdated?.Invoke(item);
		}

		/// <summary> This method do not need to care about the events, just add the data and tell if successful. </summary>
		protected abstract bool AddToDataset(T item);

		/// <summary> This method do not need to care about the events, just tell if the data can remove. </summary>
		protected abstract bool IsRemovable(T item);

		/// <summary> This method do not need to care about the events, just remove the data. </summary>
		protected abstract void RemoveFromDataset(T item);

		/// <summary> Used when a data is updated. </summary>
		protected abstract bool NeedToRemove(T item);

		public abstract IEnumerator<T> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary> Clear itself. </summary>
		public virtual void Dispose() => Clear();
	}
}