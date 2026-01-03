#nullable enable

using System;
using System.Collections.Generic;
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

		public bool AlwaysRebuildLayout { get; set; } = true;

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
		public bool AlwaysRebuildLayout { get; set; } = true;

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
}