#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace T3Framework.Runtime.ECS
{
	public class DerivedComponent<TLocator, T> : IComponent<T> where TLocator : struct, IDataLocator<T>
	{
		public TLocator Locator { get; internal set; }

		public T Model { get; internal set; }

		internal event EventHandler? OnExternalUpdated;

		public event EventHandler? OnComponentUpdated;

		public DerivedComponent(TLocator locator, T model)
		{
			Locator = locator;
			Model = model;
		}

		internal void InternalNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public void UpdateNotify() => OnExternalUpdated?.Invoke(this, EventArgs.Empty);

		public void UpdateModel(Action<T> action)
		{
			action.Invoke(Model);
			UpdateNotify(); // Only report to its dataset, dataset then calls InternalNotify() to broadcast to all
		}
	}

	public class DerivedDataset<TParent, TData, TLocator> : IReadOnlyDataset<DerivedComponent<TLocator, TData>>
		where TParent : IComponent where TLocator : struct, IDataLocator<TData>
	{
		private readonly Func<TParent, IEnumerable<TLocator>> locatorFactory;
		private readonly Dictionary<TParent, HashSet<DerivedComponent<TLocator, TData>>> map = new();
		private readonly Dictionary<DerivedComponent<TLocator, TData>, TParent> parentMap = new();
		private TParent? parentBuffer;

		public event Action<DerivedComponent<TLocator, TData>>? OnDataAdded;
		public event Action<DerivedComponent<TLocator, TData>>? OnDataAddedInherit;
		public event Action<DerivedComponent<TLocator, TData>>? BeforeDataRemoved;
		public event Action<DerivedComponent<TLocator, TData>>? BeforeDataRemovedInherit;
		public event Action<DerivedComponent<TLocator, TData>>? OnDataUpdated;

		public IReadOnlyDataset<TParent> ParentDataset { get; }

		public int Count => map.Count;

		public TParent? this[DerivedComponent<TLocator, TData> child] => parentMap.GetValueOrDefault(child);

		public IReadOnlyCollection<DerivedComponent<TLocator, TData>> this[TParent parent] =>
			map.TryGetValue(parent, out var result) ? result : Array.Empty<DerivedComponent<TLocator, TData>>();

		public DerivedDataset(
			IReadOnlyDataset<TParent> parentDataset,
			Func<TParent, IEnumerable<TLocator>> locatorFactory)
		{
			ParentDataset = parentDataset;
			this.locatorFactory = locatorFactory;
			parentDataset.OnDataAdded += OnParentDataAdded;
			parentDataset.OnDataAddedInherit += OnParentDataAddedInherit;
			parentDataset.BeforeDataRemoved += BeforeParentDataRemoved;
			parentDataset.BeforeDataRemovedInherit += BeforeParentDataRemovedInherit;
			parentDataset.OnDataUpdated += OnParentDataUpdated;
			foreach (var parent in parentDataset) OnParentDataAddedInherit(parent);
		}

		public bool Contains(DerivedComponent<TLocator, TData> component) => parentMap.ContainsKey(component);

		// This method is for that user can only update data in DerivedComponent. In this case, only the
		// corresponding dataset will receive the update event, but it should also report it to the parent.
		private void OnDataExternalUpdated(object sender, EventArgs e)
		{
			var component = sender as DerivedComponent<TLocator, TData>;
			var parent = parentMap.GetValueOrDefault(component!);
			// Buffer the parent so when the dataset receives parent's update event notified by itself,
			// it will skip this one.
			parentBuffer = parent;
			component!.InternalNotify();
			parent?.UpdateNotify();
		}

		private void OnParentDataAdded(TParent parent)
		{
			foreach (var component in map[parent]) OnDataAdded?.Invoke(component);
		}

		private void OnParentDataAddedInherit(TParent parent)
		{
			map[parent] = new();
			foreach (var locator in locatorFactory.Invoke(parent))
			{
				var data = locator.GetData();
				if (data is null) continue;
				var component = new DerivedComponent<TLocator, TData>(locator, data);
				map[parent].Add(component);
				parentMap[component] = parent;
				component.OnExternalUpdated += OnDataExternalUpdated;
				OnDataAddedInherit?.Invoke(component);
			}
		}

		private void BeforeParentDataRemoved(TParent parent)
		{
			if (map.TryGetValue(parent, out var set))
			{
				foreach (var component in set) BeforeDataRemoved?.Invoke(component);
			}
		}

		private void BeforeParentDataRemovedInherit(TParent parent)
		{
			if (map.TryGetValue(parent, out var set))
			{
				foreach (var component in set)
				{
					BeforeDataRemovedInherit?.Invoke(component);
					parentMap.Remove(component);
				}
			}

			map.Remove(parent);
		}

		private void OnParentDataUpdated(TParent parent)
		{
			if (ReferenceEquals(parent, parentBuffer))
			{
				parentBuffer = default;
				return;
			}

			if (!map.TryGetValue(parent, out var components)) return;
			var locatorSet = locatorFactory.Invoke(parent).ToHashSet();

			var toRemove = new Queue<DerivedComponent<TLocator, TData>>();
			foreach (var component in components)
			{
				// 1. Remove components with now invalid locator
				if (!locatorSet.Contains(component.Locator))
				{
					toRemove.Enqueue(component);
				}
				// 2. Check if existing locator changes component
				else
				{
					locatorSet.Remove(component.Locator);
					var data = component.Locator.GetData();
					if (data is not null)
					{
						component.Model = data;
						component.InternalNotify();
						OnDataUpdated?.Invoke(component);
					}
				}
			}

			// 3. Add newly appeared locator
			foreach (var locator in locatorSet)
			{
				var data = locator.GetData();
				if (data is not null)
				{
					if (toRemove.TryDequeue(out var component))
					{
						component.Locator = locator;
						component.Model = data;
						component.InternalNotify();
						OnDataUpdated?.Invoke(component);
					}
					else
					{
						var newComponent = new DerivedComponent<TLocator, TData>(locator, data);
						newComponent.OnExternalUpdated += OnDataExternalUpdated;
						map[parent].Add(newComponent);
						parentMap[newComponent] = parent;
						OnDataAddedInherit?.Invoke(newComponent);
						OnDataAdded?.Invoke(newComponent);
					}
				}
			}

			foreach (var component in toRemove)
			{
				component.OnExternalUpdated -= OnDataExternalUpdated;
				BeforeDataRemoved?.Invoke(component);
				BeforeDataRemovedInherit?.Invoke(component);
				parentMap.Remove(component);
			}

			components.ExceptWith(toRemove);
		}

		public IEnumerator<DerivedComponent<TLocator, TData>> GetEnumerator() => parentMap.Keys.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Dispose()
		{
			ParentDataset.OnDataAddedInherit -= OnParentDataAddedInherit;
			ParentDataset.BeforeDataRemovedInherit -= BeforeParentDataRemovedInherit;
			ParentDataset.OnDataUpdated -= OnParentDataUpdated;
			foreach (var component in map.Values.SelectMany(set => set))
			{
				component.OnExternalUpdated -= OnDataExternalUpdated;
			}

			map.Clear();
			parentMap.Clear();
		}
	}
}