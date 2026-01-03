#nullable enable

using System;
using System.Linq;
using UnityEngine;

namespace T3Framework.Runtime.ECS
{
	public class SubViewPool<T, TClass> : SubDataset<T, TClass>, IViewPool<T> where T : IComponent
	{
		private readonly IViewPool<T> parentDataset;

		public Transform DefaultTransform => parentDataset.DefaultTransform;

		public PrefabHandler? this[T item] => Contains(item) ? parentDataset[item] : null;

		public T? this[PrefabHandler handler]
		{
			get
			{
				var data = parentDataset[handler];
				if (data is null || !IsDataOfClass(data)) return default;
				return data;
			}
		}

		public SubViewPool(IViewPool<T> parentDataset, IClassifier<TClass> classifier, params TClass[] targetClasses) :
			base(parentDataset, classifier, targetClasses)
		{
			this.parentDataset = parentDataset;
			this.parentDataset.OnCreate += OnParentCreate;
			this.parentDataset.OnGet += OnParentGet;
			this.parentDataset.OnRelease += OnParentRelease;
			this.parentDataset.OnDestroy += OnParentDestroy;
		}

		public bool Add(T item) => IsDataOfClass(item) && parentDataset.Add(item);

		public bool Remove(T item) => IsDataOfClass(item) && parentDataset.Remove(item);

		public void Clear()
		{
			var data = this.ToArray();
			foreach (var item in data) parentDataset.Remove(item);
		}

		public bool IsGetActive
		{
			get => parentDataset.IsGetActive;
			set => parentDataset.IsGetActive = value;
		}

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;

		public override void Dispose()
		{
			base.Dispose();
			parentDataset.OnCreate -= OnParentCreate;
			parentDataset.OnGet -= OnParentGet;
			parentDataset.OnRelease -= OnParentRelease;
			parentDataset.OnDestroy -= OnParentDestroy;
		}

		private void OnParentCreate(object sender, PrefabHandler handler)
		{
			var data = parentDataset[handler];
			if (data is not null && IsDataOfClass(data)) OnCreate?.Invoke(this, handler);
		}

		private void OnParentGet(object sender, PrefabHandler handler)
		{
			var data = parentDataset[handler];
			if (data is not null && IsDataOfClass(data)) OnGet?.Invoke(this, handler);
		}

		private void OnParentRelease(object sender, PrefabHandler handler)
		{
			var data = parentDataset[handler];
			if (data is not null && IsDataOfClass(data)) OnRelease?.Invoke(this, handler);
		}

		private void OnParentDestroy(object sender, PrefabHandler handler)
		{
			var data = parentDataset[handler];
			if (data is not null && IsDataOfClass(data)) OnDestroy?.Invoke(this, handler);
		}
	}
}