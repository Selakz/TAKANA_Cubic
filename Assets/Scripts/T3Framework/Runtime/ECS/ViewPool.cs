#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using Object = UnityEngine.Object;

namespace T3Framework.Runtime.ECS
{
	[Serializable]
	public struct ViewPoolInstaller
	{
		[SerializeField] private PrefabObject prefab;
		[SerializeField] private Transform defaultTransform;

		public RegistrationBuilder Register<TPool, T>(IContainerBuilder builder, Lifetime lifetime)
			where TPool : IViewPool<T> where T : IComponent
		{
			return builder.Register<TPool>(lifetime)
				.WithParameter("prefab", prefab)
				.WithParameter("defaultTransform", defaultTransform)
				.As<IViewPool<T>>();
		}
	}

	[Serializable]
	public struct ClassViewPoolInstaller<TClass>
	{
		[SerializeField] private InspectorDictionary<TClass, PrefabObject> prefabs;
		[SerializeField] private Transform defaultTransform;

		public RegistrationBuilder Register<TPool, T>
			(IContainerBuilder builder, Lifetime lifetime, IClassifier<TClass> classifier)
			where TPool : IViewPool<T> where T : IComponent
		{
			return builder.Register<TPool>(lifetime)
				.WithParameter("prefabs", prefabs.Value)
				.WithParameter("defaultTransform", defaultTransform)
				.WithParameter("classifier", classifier)
				.As<IViewPool<T>>();
		}
	}

	public interface IViewPool<T> : IDataset<T> where T : IComponent
	{
		public bool IsGetActive { get; set; }

		public Transform DefaultTransform { get; }

		public PrefabHandler? this[T item] { get; }
		public T? this[PrefabHandler handler] { get; }

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;
	}

	public class ViewPool<T> : RootDataset<T>, IViewPool<T> where T : IComponent
	{
		// Public
		public bool IsGetActive { get; set; } = true;

		public Transform DefaultTransform { get; }

		public PrefabHandler? this[T item] => handlerMap.GetValueOrDefault(item);

		public T? this[PrefabHandler handler] => dataMap.GetValueOrDefault(handler);

		public override int Count => handlerMap.Count;

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;

		// Private
		private readonly IObjectResolver resolver;
		private readonly ObjectPool<PrefabHandler> pool;
		private readonly Dictionary<T, PrefabHandler> handlerMap = new();
		private readonly Dictionary<PrefabHandler, T> dataMap = new();

		// Defined Functions
		public ViewPool(IObjectResolver resolver, PrefabObject prefab, Transform defaultTransform)
		{
			this.resolver = resolver;
			DefaultTransform = defaultTransform;
			pool = NewPool(prefab);
		}

		public override bool Contains(T item) => handlerMap.ContainsKey(item);

		protected override bool AddToDataset(T item)
		{
			if (handlerMap.ContainsKey(item)) return false;
			var handler = pool.Get();
			handlerMap[item] = handler;
			dataMap[handler] = item;
			OnGet?.Invoke(this, handler);
			return true;
		}

		protected override bool IsRemovable(T item) => handlerMap.ContainsKey(item);

		protected override void RemoveFromDataset(T item)
		{
			if (!handlerMap.TryGetValue(item, out var handler)) return;
			OnRelease?.Invoke(this, handler);
			handlerMap.Remove(item);
			dataMap.Remove(handler);
			pool.Release(handler);
		}

		protected override bool NeedToRemove(T item) => false;

		public override IEnumerator<T> GetEnumerator() => handlerMap.Keys.GetEnumerator();

		private ObjectPool<PrefabHandler> NewPool(PrefabObject prefab)
		{
			return new(
				() =>
				{
					var handler = prefab.Instantiate(resolver, DefaultTransform, false);
					OnCreate?.Invoke(this, handler);
					return handler;
				},
				handler => { handler.gameObject.SetActive(IsGetActive); },
				handler =>
				{
					if (handler == null) return;
					handler.gameObject.SetActive(false);
					handler.transform.SetParent(DefaultTransform, false);
				},
				handler =>
				{
					if (handler == null) return;
					OnDestroy?.Invoke(this, handler);
					Object.Destroy(handler.gameObject);
				},
				defaultCapacity: 0);
		}
	}

	public class ViewPool<T, TClass> : RootDataset<T>, IViewPool<T> where T : IComponent
	{
		// Public
		public bool IsGetActive { get; set; } = true;

		public Transform DefaultTransform { get; }

		public PrefabHandler? this[T item] => handlerMap.GetValueOrDefault(item);

		public T? this[PrefabHandler handler] => dataMap.GetValueOrDefault(handler);

		public override int Count => handlerMap.Count;

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;

		// Private
		private readonly IObjectResolver resolver;
		private readonly IClassifier<TClass> classifier;
		private readonly Dictionary<TClass, PrefabObject> prefabs;
		private readonly Dictionary<TClass, ObjectPool<PrefabHandler>> pools = new();
		private readonly Dictionary<T, TClass> classMap = new();
		private readonly Dictionary<T, PrefabHandler> handlerMap = new();
		private readonly Dictionary<PrefabHandler, T> dataMap = new();

		// Defined Functions
		public ViewPool(IObjectResolver resolver, IClassifier<TClass> classifier,
			Dictionary<TClass, PrefabObject> prefabs, Transform defaultTransform)
		{
			this.resolver = resolver;
			this.classifier = classifier;
			this.prefabs = prefabs;
			this.DefaultTransform = defaultTransform;
		}

		public override bool Contains(T item) => handlerMap.ContainsKey(item);

		protected override bool AddToDataset(T item)
		{
			if (handlerMap.ContainsKey(item)) return false;
			var classType = classifier.Classify(item);
			if (classType is null) return false;
			var pair = prefabs.FirstOrDefault(p => classifier.IsSubType(p.Key, classType));
			if (pair.Key is null) return false;
			classMap[item] = pair.Key;
			if (!pools.ContainsKey(pair.Key)) pools[pair.Key] = NewPool(pair.Value);
			var handler = pools[pair.Key].Get();
			handlerMap[item] = handler;
			dataMap[handler] = item;
			OnGet?.Invoke(this, handler);
			return true;
		}

		protected override bool IsRemovable(T item) => handlerMap.ContainsKey(item);

		protected override void RemoveFromDataset(T item)
		{
			if (!handlerMap.TryGetValue(item, out var handler)) return;
			OnRelease?.Invoke(this, handler);
			handlerMap.Remove(item);
			dataMap.Remove(handler);
			classMap.Remove(item, out var classType);
			if (pools.TryGetValue(classType, out var pool)) pool.Release(handler);
		}

		protected override bool NeedToRemove(T item)
		{
			var oldClassType = classMap[item];
			var newClassType = classifier.Classify(item);
			if (newClassType is null) return true;
			if (!classifier.IsSubType(oldClassType, newClassType))
			{
				RemoveFromDataset(item);
				AddToDataset(item);
			}

			return false;
		}

		public override IEnumerator<T> GetEnumerator() => handlerMap.Keys.GetEnumerator();

		private ObjectPool<PrefabHandler> NewPool(PrefabObject prefab)
		{
			return new(
				() =>
				{
					var handler = prefab.Instantiate(resolver, DefaultTransform, false);
					OnCreate?.Invoke(this, handler);
					return handler;
				},
				handler => handler.gameObject.SetActive(IsGetActive),
				handler =>
				{
					if (handler == null) return;
					handler.gameObject.SetActive(false);
					handler.transform.SetParent(DefaultTransform, false);
				},
				handler =>
				{
					if (handler == null) return;
					OnDestroy?.Invoke(this, handler);
					Object.Destroy(handler.gameObject);
				},
				defaultCapacity: 0);
		}
	}
}