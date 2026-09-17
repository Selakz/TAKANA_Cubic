#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.ECS;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace MusicGame.Utility.UI
{
	public class ViewPoolFancyScrollViewContext<T> where T : IComponent
	{
		public ViewPoolFancyScrollView<T> View { get; set; } = default!;
	}

	public class ViewPoolFancyScrollView<T> : FancyScrollView<T, ViewPoolFancyScrollViewContext<T>>, IViewPool<T>
		where T : IComponent
	{
		// Serializable and Public
		[SerializeField] private Scroller scroller = default!;
		[SerializeField] private GameObject cellPrefab = default!;

		public bool IsGetActive { get; set; } = true;

		public Transform DefaultTransform => cellContainer;

		public PrefabHandler? this[T item] => itemToHandler.GetValueOrDefault(item);

		public T? this[PrefabHandler handler] => handlerToItem.GetValueOrDefault(handler);

		public int Count => items.Count;

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;

		public event Action<T>? OnDataAdded;
		public event Action<T>? OnDataAddedInherit;
		public event Action<T>? BeforeDataRemoved;
		public event Action<T>? BeforeDataRemovedInherit;
		public event Action<T>? OnDataUpdated;

		// Private
		private readonly List<T> items = new();
		private readonly Dictionary<T, PrefabHandler> itemToHandler = new();
		private readonly Dictionary<PrefabHandler, T> handlerToItem = new();

		protected override GameObject CellPrefab => cellPrefab;

		// Defined Functions
		public bool Add(T item)
		{
			if (items.Contains(item)) return false;
			items.Add(item);
			item.OnComponentUpdated += OnComponentUpdated;
			UpdateContents(items);
			OnDataAddedInherit?.Invoke(item);
			OnDataAdded?.Invoke(item);
			return true;
		}

		public bool Remove(T item)
		{
			if (!items.Contains(item)) return false;
			item.OnComponentUpdated -= OnComponentUpdated;
			BeforeDataRemoved?.Invoke(item);
			BeforeDataRemovedInherit?.Invoke(item);
			items.Remove(item);
			UpdateContents(items);
			return true;
		}

		public void Clear()
		{
			foreach (var item in items.ToList()) Remove(item);
		}

		public bool Contains(T item) => items.Contains(item);

		public void JumpTo(int index) => scroller.Position = index;

		public void ScrollTo(int index, float duration) => scroller.ScrollTo(index, duration);

		public IEnumerator<T> GetEnumerator() => items.GetEnumerator();

		protected override void Initialize()
		{
			base.Initialize();
			Context.View = this;
			scroller.OnValueChanged(UpdatePosition);
		}

		protected override void UpdateContents(IList<T> itemsSource)
		{
			base.UpdateContents(itemsSource);
			scroller.SetTotalCount(itemsSource.Count);
			if (itemsSource.Count == 0) scroller.Position = 0f;
		}

		// For FancyCell to call
		public void NotifyCellCreated(PrefabHandler handler) => OnCreate?.Invoke(this, handler);

		public void NotifyCellAssigned(PrefabHandler handler, T item)
		{
			if (handlerToItem.TryGetValue(handler, out var oldItem))
			{
				if (ReferenceEquals(oldItem, item)) return;
				ReleaseFromCell(handler, oldItem);
			}

			handlerToItem[handler] = item;
			itemToHandler[item] = handler;
			OnGet?.Invoke(this, handler);
		}

		public void NotifyCellReleased(PrefabHandler handler)
		{
			if (!handlerToItem.TryGetValue(handler, out var item)) return;
			ReleaseFromCell(handler, item);
		}

		private void ReleaseFromCell(PrefabHandler handler, T item)
		{
			handlerToItem.Remove(handler);
			itemToHandler.Remove(item);
			OnRelease?.Invoke(this, handler);
		}

		// Event Handlers
		private void OnComponentUpdated(object sender, EventArgs _)
		{
			if (sender is T item) OnDataUpdated?.Invoke(item);
		}

		// System Functions
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Dispose()
		{
			foreach (var item in items) item.OnComponentUpdated -= OnComponentUpdated;
			items.Clear();
			itemToHandler.Clear();
			handlerToItem.Clear();
		}
	}
}