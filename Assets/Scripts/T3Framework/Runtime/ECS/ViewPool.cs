#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;
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

		public PrefabObject Prefab
		{
			get => pool.Prefab;
			set => pool.UpdatePrefab(value);
		}

		public Transform DefaultTransform { get; }

		public PrefabHandler? this[T item] => handlerMap.TryGetValue(item, out var handler) ? handler.Value : null;

		public T? this[PrefabHandler handler]
		{
			get
			{
				// Usually, the handler's version is not out of date.
				if (dataMap.TryGetValue(new VersionHandler(handler, pool.Version), out var item)) return item;
				// If prefab is changed and need to get old handlers, fall back to traversal.
				foreach (var versionHandler in dataMap.Keys)
				{
					if (versionHandler.Value == handler) return dataMap[versionHandler];
				}

				return default;
			}
		}

		public override int Count => handlerMap.Count;

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;

		// Private
		private readonly ViewObjectPool pool;
		private readonly Dictionary<T, VersionHandler> handlerMap = new();
		private readonly Dictionary<VersionHandler, T> dataMap = new();

		// Defined Functions
		public ViewPool(IObjectResolver resolver, PrefabObject prefab, Transform defaultTransform)
		{
			DefaultTransform = defaultTransform;
			pool = new ViewObjectPool(resolver, prefab, defaultTransform);
			pool.OnCreate += PoolOnCreate;
			pool.OnDestroy += PoolOnDestroy;
		}

		private void PoolOnCreate(object sender, VersionHandler e) => OnCreate?.Invoke(this, e.Value);

		private void PoolOnDestroy(object sender, VersionHandler e) => OnDestroy?.Invoke(this, e.Value);

		public override bool Contains(T item) => handlerMap.ContainsKey(item);

		protected override bool AddToDataset(T item)
		{
			if (handlerMap.ContainsKey(item)) return false;
			var handler = pool.Get();
			handlerMap[item] = handler;
			dataMap[handler] = item;
			OnGet?.Invoke(this, handler.Value);
			handler.Value.gameObject.SetActive(IsGetActive);
			return true;
		}

		protected override bool IsRemovable(T item) => handlerMap.ContainsKey(item);

		protected override void RemoveFromDataset(T item)
		{
			if (!handlerMap.TryGetValue(item, out var handler)) return;
			OnRelease?.Invoke(this, handler.Value);
			handlerMap.Remove(item);
			dataMap.Remove(handler);
			pool.Release(handler);
		}

		protected override bool NeedToRemove(T item) => false;

		public override IEnumerator<T> GetEnumerator() => handlerMap.Keys.GetEnumerator();

		public override void Dispose()
		{
			base.Dispose();
			pool.OnCreate -= PoolOnCreate;
			pool.OnDestroy -= PoolOnDestroy;
			pool.Dispose();
		}
	}

	public class ViewPool<T, TClass> : RootDataset<T>, IViewPool<T> where T : IComponent
	{
		// Public
		public bool IsGetActive { get; set; } = true;

		/// <summary> Change the prefab map will not influence current existing instances. </summary>
		public IReadOnlyDictionary<TClass, PrefabObject> Prefabs
		{
			get => prefabs;
			set
			{
				foreach (var key in prefabs.Keys)
				{
					if (!value.ContainsKey(key))
						throw new ArgumentException(
							$"New prefabs dictionary must contain all existing keys. Missing key: {key}",
							nameof(value));
				}

				foreach (var pair in value)
				{
					if (pools.TryGetValue(pair.Key, out var pool))
						pool.UpdatePrefab(pair.Value);
				}

				prefabs = value;
			}
		}

		public Transform DefaultTransform { get; }

		public PrefabHandler? this[T item] => handlerMap.TryGetValue(item, out var handler) ? handler.Value : null;

		public T? this[PrefabHandler handler]
		{
			get
			{
				// Usually, the handler's version is not out of date.
				if (dataMap.TryGetValue(new VersionHandler(handler, Version), out var item)) return item;
				// If prefab is changed and need to get old handlers, fall back to traversal.
				foreach (var versionHandler in dataMap.Keys)
				{
					if (versionHandler.Value == handler) return dataMap[versionHandler];
				}

				return default;
			}
		}

		public override int Count => handlerMap.Count;

		public event EventHandler<PrefabHandler>? OnCreate;
		public event EventHandler<PrefabHandler>? OnGet;
		public event EventHandler<PrefabHandler>? OnRelease;
		public event EventHandler<PrefabHandler>? OnDestroy;

		// Private
		private readonly IObjectResolver resolver;
		private readonly IClassifier<TClass> classifier;
		private readonly Dictionary<TClass, ViewObjectPool> pools = new();
		private readonly Dictionary<T, TClass> classMap = new();
		private readonly Dictionary<T, VersionHandler> handlerMap = new();
		private readonly Dictionary<VersionHandler, T> dataMap = new();

		private IReadOnlyDictionary<TClass, PrefabObject> prefabs;

		private int Version => pools.Values.Select(pool => pool.Version).FirstOrDefault();

		// Defined Functions
		public ViewPool(IObjectResolver resolver, IClassifier<TClass> classifier,
			IReadOnlyDictionary<TClass, PrefabObject> prefabs, Transform defaultTransform)
		{
			this.resolver = resolver;
			this.classifier = classifier;
			this.prefabs = prefabs;
			DefaultTransform = defaultTransform;
		}

		private void PoolOnCreate(object sender, VersionHandler e) => OnCreate?.Invoke(this, e.Value);

		private void PoolOnDestroy(object sender, VersionHandler e) => OnDestroy?.Invoke(this, e.Value);

		public override bool Contains(T item) => handlerMap.ContainsKey(item);

		protected override bool AddToDataset(T item)
		{
			if (handlerMap.ContainsKey(item)) return false;
			var classType = classifier.Classify(item);
			if (classType is null) return false;
			var pair = Prefabs.FirstOrDefault(p => classifier.IsSubType(p.Key, classType));
			if (pair.Key is null) return false;
			classMap[item] = pair.Key;
			if (!pools.ContainsKey(pair.Key))
			{
				var pool = new ViewObjectPool(resolver, pair.Value, DefaultTransform, Version);
				pool.OnCreate += PoolOnCreate;
				pool.OnDestroy += PoolOnDestroy;
				pools[pair.Key] = pool;
			}

			var handler = pools[pair.Key].Get();
			handlerMap[item] = handler;
			dataMap[handler] = item;
			OnGet?.Invoke(this, handler.Value);
			handler.Value.gameObject.SetActive(IsGetActive);
			return true;
		}

		protected override bool IsRemovable(T item) => handlerMap.ContainsKey(item);

		protected override void RemoveFromDataset(T item)
		{
			if (!handlerMap.TryGetValue(item, out var handler)) return;
			OnRelease?.Invoke(this, handler.Value);
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

		public override void Dispose()
		{
			base.Dispose();
			foreach (var pool in pools.Values)
			{
				pool.OnCreate -= PoolOnCreate;
				pool.OnDestroy -= PoolOnDestroy;
				pool.Dispose();
			}
		}
	}

	internal struct VersionHandler : IEquatable<VersionHandler>
	{
		public PrefabHandler Value { get; set; }

		public int Version { get; set; }

		public VersionHandler(PrefabHandler handler, int version)
		{
			Value = handler;
			Version = version;
		}

		public bool Equals(VersionHandler other) => Value == other.Value && Version == other.Version;

		public override bool Equals(object? obj) => obj is VersionHandler other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(Value, Version);

		public static bool operator ==(VersionHandler left, VersionHandler right) => left.Equals(right);

		public static bool operator !=(VersionHandler left, VersionHandler right) => !left.Equals(right);
	}

	internal class ViewObjectPool : IDisposable
	{
		private readonly IObjectResolver resolver;
		private readonly Stack<VersionHandler> stack = new();
		private readonly Transform defaultTransform;

		public PrefabObject Prefab { get; private set; }

		public int Version { get; private set; } = 0;

		public event EventHandler<VersionHandler>? OnCreate;
		public event EventHandler<VersionHandler>? OnDestroy;

		public ViewObjectPool(
			IObjectResolver resolver, PrefabObject prefab, Transform defaultTransform, int initialVersion = 0)
		{
			this.resolver = resolver;
			this.defaultTransform = defaultTransform;
			Prefab = prefab;
			Version = initialVersion;
		}

		public void UpdatePrefab(PrefabObject prefab)
		{
			Prefab = prefab;
			Version++;
		}

		public VersionHandler Get()
		{
			while (stack.Count > 0)
			{
				var handler = stack.Pop();
				if (handler.Value == null) continue;
				if (handler.Version == Version) return handler;
				else Object.Destroy(handler.Value.gameObject);
			}

			var newHandler = new VersionHandler(Prefab.Instantiate(resolver, defaultTransform, false), Version);
			OnCreate?.Invoke(this, newHandler);
			return newHandler;
		}

		public void Release(VersionHandler handler)
		{
			if (handler.Value == null) return;
			if (handler.Version != Version)
			{
				OnDestroy?.Invoke(this, handler);
				Object.Destroy(handler.Value.gameObject);
				return;
			}

			handler.Value.gameObject.SetActive(false);
			handler.Value.transform.SetParent(defaultTransform, false);
			stack.Push(handler);
		}

		public void DestroyInactive()
		{
			while (stack.Count > 0)
			{
				var handler = stack.Pop();
				if (handler.Value == null) return;
				OnDestroy?.Invoke(this, handler);
				Object.Destroy(handler.Value.gameObject);
			}
		}

		public void Dispose()
		{
			OnCreate = null;
			OnDestroy = null;
			DestroyInactive();
		}
	}
}