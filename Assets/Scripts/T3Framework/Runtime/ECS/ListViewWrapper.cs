#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Runtime.ECS
{
	/// <summary> Automatically sorts the views. </summary>
	public class ListViewAutoSorter<T> : IDisposable where T : IComponent
	{
		private readonly Transform parentTransform;

		// Serializable and Public
		public Comparison<T> ListSorter
		{
			get => listSorter;
			set
			{
				listSorter = value;
				Sort();
			}
		}

		public bool AlwaysRebuildLayout { get; set; } = false;

		public IViewPool<T> ViewPool { get; }

		// Private
		private Comparison<T> listSorter = Comparer<T>.Default.Compare;

		// Defined Functions
		public ListViewAutoSorter(IViewPool<T> viewPool)
		{
			ViewPool = viewPool;
			parentTransform = ViewPool.DefaultTransform;
			ViewPool.OnGet += OnGet;
			ViewPool.OnDataUpdated += OnDataUpdated;
			ViewPool.OnRelease += OnRelease;
		}

		public int GetSiblingIndex(T item)
		{
			var handler = ViewPool[item];
			return handler?.transform.GetSiblingIndex() ?? -1;
		}

		public void RebuildLayout()
		{
			if (parentTransform is RectTransform rectTransform)
				LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		private void Sort()
		{
			List<T> keys = new List<T>(ViewPool);
			keys.Sort(ListSorter);
			for (int i = 0; i < keys.Count; i++)
			{
				ViewPool[keys[i]]?.transform.SetSiblingIndex(i);
			}

			if (AlwaysRebuildLayout) RebuildLayout();
		}

		// Event Handlers
		private void OnGet(object sender, PrefabHandler handler) => Sort();

		private void OnDataUpdated(T data) => Sort();

		private void OnRelease(object sender, PrefabHandler handler)
		{
			if (AlwaysRebuildLayout) RebuildLayout();
		}

		public void Dispose()
		{
			ViewPool.OnGet -= OnGet;
			ViewPool.OnRelease -= OnRelease;
		}
	}

	/// <summary> Manually sorts the views. </summary>
	public class ListViewManualSorter<T> : IDisposable where T : IComponent
	{
		private readonly Transform parentTransform;

		// Serializable and Public
		public bool AlwaysRebuildLayout { get; set; } = false;

		public IViewPool<T> ViewPool { get; }

		// Defined Functions
		public ListViewManualSorter(IViewPool<T> viewPool)
		{
			ViewPool = viewPool;
			parentTransform = ViewPool.DefaultTransform;
			ViewPool.OnGet += OnGet;
			ViewPool.OnRelease += OnRelease;
		}

		public int GetSiblingIndex(T item)
		{
			var handler = ViewPool[item];
			return handler?.transform.GetSiblingIndex() ?? -1;
		}

		public void SetSiblingIndex(T item, int index)
		{
			var handler = ViewPool[item];
			handler?.transform.SetSiblingIndex(index);
		}

		public int SwapUp(T item)
		{
			var index = GetSiblingIndex(item);
			if (index <= 0) return index;
			var handler = ViewPool[item];
			handler?.transform.SetSiblingIndex(index - 1);
			return index - 1;
		}

		public int SwapDown(T item)
		{
			var index = GetSiblingIndex(item);
			if (index == -1 || index == ViewPool.Count - 1) return index;
			var handler = ViewPool[item];
			handler?.transform.SetSiblingIndex(index + 1);
			return index + 1;
		}

		public void RebuildLayout()
		{
			if (parentTransform is RectTransform rectTransform)
				LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		// Event Handlers
		private void OnGet(object sender, PrefabHandler handler)
		{
			if (AlwaysRebuildLayout) RebuildLayout();
		}

		private void OnRelease(object sender, PrefabHandler handler)
		{
			if (AlwaysRebuildLayout) RebuildLayout();
		}

		public void Dispose()
		{
			ViewPool.OnGet -= OnGet;
			ViewPool.OnRelease -= OnRelease;
		}
	}

	public class ListDatasetViewSorter<T> : IEventRegistrar where T : IComponent
	{
		public ListDataset<T> FromDataset { get; }

		public IViewPool<T> ViewPool { get; }

		public bool AlwaysRebuildLayout { get; set; } = false;

		public ListDatasetViewSorter(ListDataset<T> fromDataset, IViewPool<T> viewPool)
		{
			FromDataset = fromDataset;
			ViewPool = viewPool;
		}

		// Defined Functions
		public void Register()
		{
			ViewPool.OnGet += OnGet;
			ViewPool.OnRelease += OnRelease;
			FromDataset.OnOrderChanged += OnOrderChanged;
			Sort();
		}

		public void Unregister()
		{
			ViewPool.OnGet -= OnGet;
			ViewPool.OnRelease -= OnRelease;
			FromDataset.OnOrderChanged -= OnOrderChanged;
		}

		public void RebuildLayout()
		{
			if (ViewPool.DefaultTransform is RectTransform rectTransform)
				LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		private void Sort()
		{
			int siblingIndex = 0;
			foreach (var data in FromDataset)
			{
				if (ViewPool[data] is not { } handler) continue;
				handler.transform.SetSiblingIndex(siblingIndex);
				siblingIndex++;
			}
		}

		// Event Handlers
		private void OnGet(object sender, PrefabHandler handler)
		{
			if (ViewPool[handler] is { } data)
			{
				// The data is most possible to be the last one of the dataset
				if (ReferenceEquals(data, FromDataset[^1]))
				{
					handler.transform.SetSiblingIndex(FromDataset.Count - 1);
				}
				else Sort();
			}

			if (AlwaysRebuildLayout) RebuildLayout();
		}

		private void OnRelease(object sender, PrefabHandler handler)
		{
			if (AlwaysRebuildLayout) RebuildLayout();
		}

		private void OnOrderChanged(IOrderChangeInfo info)
		{
			switch (info)
			{
				case SwapOrderChangeInfo swapInfo:
					var si = swapInfo.Index1;
					var sj = swapInfo.Index2;
					var sHandler1 = ViewPool[FromDataset[si]];
					var sHandler2 = ViewPool[FromDataset[sj]];
					if (sHandler1 is not null && sHandler2 is not null)
					{
						if (si > sj) (sHandler1, sHandler2) = (sHandler2, sHandler1);
						var siblingIndex1 = sHandler1.transform.GetSiblingIndex();
						sHandler1.transform.SetSiblingIndex(sHandler2.transform.GetSiblingIndex());
						sHandler2.transform.SetSiblingIndex(siblingIndex1);
					}
					else if (sHandler1 is not null || sHandler2 is not null) Sort();

					break;
				case MoveOrderChangeInfo moveInfo:
					var mi = moveInfo.NewIndex;
					if (ViewPool[FromDataset[mi]] is not { } mHandler) break;
					if (mi == 0) mHandler.transform.SetSiblingIndex(0);
					else if (mi == FromDataset.Count - 1) mHandler.transform.SetSiblingIndex(FromDataset.Count - 1);
					else Sort();
					break;
				default:
					Sort();
					break;
			}
		}
	}
}