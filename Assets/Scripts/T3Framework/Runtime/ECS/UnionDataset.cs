#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace T3Framework.Runtime.ECS
{
	public class UnionDataset<T> : IReadOnlyDataset<T> where T : IComponent
	{
		private readonly HashSet<IDataset<T>> datasets = new();

		public bool AvoidDuplicates { get; set; } = false;

		public event Action<T>? OnDataAdded;
		public event Action<T>? OnDataAddedInherit;
		public event Action<T>? BeforeDataRemoved;
		public event Action<T>? BeforeDataRemovedInherit;
		public event Action<T>? OnDataUpdated;

		public UnionDataset()
		{
		}

		public UnionDataset(params IDataset<T>[] datasets)
		{
			foreach (var dataset in datasets) AddDataset(dataset);
		}

		public int Count => datasets.Aggregate(0, (sum, dataset) => sum + dataset.Count);

		public bool Contains(T component) => datasets.Any(dataset => dataset.Contains(component));

		public bool AddDataset(IDataset<T> dataset)
		{
			if (!datasets.Add(dataset)) return false;

			dataset.OnDataAddedInherit += OnChildDataAdded;
			dataset.BeforeDataRemovedInherit += OnChildDataRemoved;
			dataset.OnDataUpdated += OnChildDataUpdated;
			foreach (var item in dataset)
			{
				OnDataAddedInherit?.Invoke(item);
				OnDataAdded?.Invoke(item);
			}

			return true;
		}

		public bool RemoveDataset(IDataset<T> dataset)
		{
			if (!datasets.Remove(dataset)) return false;

			dataset.OnDataAddedInherit -= OnChildDataAdded;
			dataset.BeforeDataRemovedInherit -= OnChildDataRemoved;
			dataset.OnDataUpdated -= OnChildDataUpdated;

			foreach (var item in dataset)
			{
				BeforeDataRemoved?.Invoke(item);
				BeforeDataRemovedInherit?.Invoke(item);
			}

			return true;
		}

		public bool ContainsDataset(IDataset<T> dataset) => datasets.Contains(dataset);

		public IEnumerator<T> GetEnumerator()
		{
			HashSet<T>? seen = AvoidDuplicates ? new HashSet<T>() : null;
			foreach (var dataset in datasets)
			{
				foreach (var item in dataset)
				{
					if (!AvoidDuplicates || seen!.Add(item)) yield return item;
				}
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private void OnChildDataAdded(T item)
		{
			OnDataAddedInherit?.Invoke(item);
			OnDataAdded?.Invoke(item);
		}

		private void OnChildDataRemoved(T item)
		{
			BeforeDataRemoved?.Invoke(item);
			BeforeDataRemovedInherit?.Invoke(item);
		}

		private void OnChildDataUpdated(T item) => OnDataUpdated?.Invoke(item);

		public void Dispose()
		{
			foreach (var dataset in datasets)
			{
				dataset.OnDataAddedInherit -= OnChildDataAdded;
				dataset.BeforeDataRemovedInherit -= OnChildDataRemoved;
				dataset.OnDataUpdated -= OnChildDataUpdated;
			}

			datasets.Clear();
		}
	}
}