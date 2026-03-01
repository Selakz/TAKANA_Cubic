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

		private Dictionary<object, Dictionary<HashSet<object>, IReadOnlyDataset<T>>>? subDatasets = new();

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

		public IReadOnlyDataset<T> SubDataset<TClass>(IClassifier<TClass> classifier, params TClass[] targetClasses)
		{
			subDatasets ??= new();

			if (!subDatasets.ContainsKey(classifier))
				subDatasets.Add(classifier, new(SetComparer<object>.Instance));
			var classes = new HashSet<object>(targetClasses.Cast<object>());
			var datasets = subDatasets[classifier];
			if (!datasets.ContainsKey(classes))
				datasets.Add(classes, new SubDataset<T, TClass>(this, classifier, targetClasses));
			return datasets[classes];
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

		private class SetComparer<T1> : IEqualityComparer<HashSet<T1>>
		{
			public static SetComparer<T1> Instance => new Lazy<SetComparer<T1>>(() => new SetComparer<T1>()).Value;

			public bool Equals(HashSet<T1> x, HashSet<T1> y) => x == y || x.SetEquals(y);

			public int GetHashCode(HashSet<T1> set)
			{
				return set.Count == 0
					? 0
					: set.Aggregate(0, (current, item) => current ^ (item?.GetHashCode() ?? 0));
			}
		}
	}
}