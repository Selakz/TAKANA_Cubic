#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public interface IOrderChangeInfo
	{
	}

	public class FallbackOrderChangeInfo : IOrderChangeInfo
	{
		public static FallbackOrderChangeInfo Instance { get; } = new();
	}

	public class SwapOrderChangeInfo : IOrderChangeInfo
	{
		public int Index1 { get; }
		public int Index2 { get; }

		public SwapOrderChangeInfo(int index1, int index2)
		{
			Index1 = index1;
			Index2 = index2;
		}
	}

	public class MoveOrderChangeInfo : IOrderChangeInfo
	{
		public int OriginalIndex { get; }

		public int NewIndex { get; }

		public MoveOrderChangeInfo(int originalIndex, int newIndex)
		{
			OriginalIndex = originalIndex;
			NewIndex = newIndex;
		}
	}

	public class ListDataset<T> : RootDataset<T> where T : IComponent
	{
		public event Action<IOrderChangeInfo>? OnOrderChanged;

		private readonly List<T> dataset = new();

		public override int Count => dataset.Count;

		public T this[int index] => dataset[index];

		public void Sort(Comparison<T> comparison)
		{
			dataset.Sort(comparison);
			OnOrderChanged?.Invoke(FallbackOrderChangeInfo.Instance);
		}

		public void Swap(int i, int j)
		{
			if (i < 0 || i >= dataset.Count || j < 0 || j >= dataset.Count || i == j) return;
			(dataset[i], dataset[j]) = (dataset[j], dataset[i]);
			OnOrderChanged?.Invoke(new SwapOrderChangeInfo(i, j));
		}

		public void MoveToTop(int i)
		{
			if (i <= 0 || i >= dataset.Count) return;
			var target = dataset[i];
			for (int j = i; j > 0; j--) dataset[j] = dataset[j - 1];
			dataset[0] = target;
			OnOrderChanged?.Invoke(new MoveOrderChangeInfo(i, 0));
		}

		public void MoveToTop(T item)
		{
			var index = dataset.IndexOf(item);
			if (index != -1) MoveToTop(IndexOf(item));
		}

		public void MoveToBottom(int i)
		{
			if (i < 0 || i >= dataset.Count - 1) return;
			var target = dataset[i];
			for (int j = i; j < dataset.Count - 1; j++) dataset[j] = dataset[j + 1];
			dataset[^1] = target;
			OnOrderChanged?.Invoke(new MoveOrderChangeInfo(i, Count - 1));
		}

		public void MoveToBottom(T item)
		{
			var index = dataset.IndexOf(item);
			if (index != -1) MoveToBottom(IndexOf(item));
		}

		public int IndexOf(T item) => dataset.IndexOf(item);

		public override bool Contains(T item) => dataset.Contains(item);

		protected override bool AddToDataset(T item)
		{
			dataset.Add(item);
			return true;
		}

		protected override bool IsRemovable(T item) => dataset.Contains(item);

		protected override void RemoveFromDataset(T item) => dataset.Remove(item);

		protected override bool NeedToRemove(T item) => false;

		public override IEnumerator<T> GetEnumerator() => dataset.GetEnumerator();
	}

	public class ListDatasetOrderRegistrar<T> : IEventRegistrar where T : IComponent
	{
		private readonly ListDataset<T> dataset;
		private readonly Action<IOrderChangeInfo> action;

		public ListDatasetOrderRegistrar(ListDataset<T> dataset, Action<IOrderChangeInfo> action)
		{
			this.dataset = dataset;
			this.action = action;
		}

		public ListDatasetOrderRegistrar(ListDataset<T> dataset, Action action) : this(dataset, _ => action.Invoke())
		{
		}

		public void Register() => dataset.OnOrderChanged += action;

		public void Unregister() => dataset.OnOrderChanged -= action;
	}
}