#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace T3Framework.Runtime.ECS
{
	/// <summary>
	/// Contains a subset of a dataset, and will automatically respond to its parent dataset's events,
	/// act as if it itself is a complete dataset.
	/// </summary>
	public class SubDataset<T, TClass> : IReadOnlyDataset<T> where T : IComponent
	{
		private readonly IDataset<T> parentDataset;
		private readonly IClassifier<TClass> classifier;
		private readonly HashSet<TClass> targetClasses = new();
		private readonly HashSet<T> components = new();

		// TODO: Dynamically add or remove classes
		public SubDataset(IDataset<T> parentDataset, IClassifier<TClass> classifier,
			params TClass[] targetClasses)
		{
			this.parentDataset = parentDataset;
			this.classifier = classifier;
			this.targetClasses.UnionWith(targetClasses);
			components.UnionWith(parentDataset.Where(IsDataOfClass));
			parentDataset.OnDataAddedInherit += OnParentDataAdded;
			parentDataset.BeforeDataRemovedInherit += OnParentDataRemoved;
			parentDataset.OnDataUpdated += OnParentDataUpdated;
		}

		public event Action<T>? OnDataAdded;
		public event Action<T>? OnDataAddedInherit;
		public event Action<T>? BeforeDataRemoved;
		public event Action<T>? BeforeDataRemovedInherit;
		public event Action<T>? OnDataUpdated;

		public int Count => components.Count;

		public bool Contains(T component) => components.Contains(component);

		private void OnParentDataAdded(T data)
		{
			if (IsDataOfClass(data))
			{
				var res = components.Add(data);
				if (res)
				{
					OnDataAddedInherit?.Invoke(data);
					OnDataAdded?.Invoke(data);
				}
			}
		}

		private void OnParentDataRemoved(T data)
		{
			if (IsDataOfClass(data))
			{
				var res = components.Contains(data);
				if (res)
				{
					BeforeDataRemoved?.Invoke(data);
					BeforeDataRemovedInherit?.Invoke(data);
					components.Remove(data);
				}
			}
		}

		private void OnParentDataUpdated(T data)
		{
			if (components.Contains(data))
			{
				if (IsDataOfClass(data))
				{
					OnDataUpdated?.Invoke(data);
				}
				else
				{
					components.Remove(data);
					BeforeDataRemoved?.Invoke(data);
					BeforeDataRemovedInherit?.Invoke(data);
				}
			}
		}

		public bool IsDataOfClass(T data) => targetClasses.Any(type => classifier.IsOfType(data, type));

		public IEnumerator<T> GetEnumerator() => components.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public virtual void Dispose()
		{
			parentDataset.OnDataAddedInherit -= OnParentDataAdded;
			parentDataset.BeforeDataRemovedInherit -= OnParentDataRemoved;
			parentDataset.OnDataUpdated -= OnParentDataUpdated;
			components.Clear();
		}
	}
}