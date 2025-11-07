#nullable enable

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace T3Framework.Runtime.Event
{
	// TODO: Use Addressable to make it like somewhat an ECS architecture
	public class LazyPrefab
	{
		public event Action<GameObject>? ActionBeforeAwake = null;

		private static readonly Transform prefabRoot;
		private readonly Lazy<GameObject> prefab;

		static LazyPrefab()
		{
			var go = new GameObject("PrefabRoot");
			Object.DontDestroyOnLoad(go);
			prefabRoot = go.transform;
		}

		public LazyPrefab(string prefabPath, string eventName, Action<GameObject>? actionBeforeAwake = null)
		{
			prefab = new Lazy<GameObject>(() =>
			{
				GameObject prefabObject = Resources.Load<GameObject>(prefabPath);
				prefabObject.SetActive(false);
				var copy = Object.Instantiate(prefabObject, prefabRoot);
				copy.name = prefabObject.name;
				EventManager.Instance.Invoke(eventName, copy);
				return copy;
			});

			ActionBeforeAwake += actionBeforeAwake;
		}

		/// <summary>
		/// Instantiate this prefab and set the instantiated object active. Direct instantiation will get inactive object.
		/// </summary>
		public GameObject Instantiate(Transform parent, Action<GameObject>? actionBeforeAwake = null)
		{
			var go = Object.Instantiate<GameObject>(this, parent);
			// TODO: Refactor prefab to register T3MonoBehaviour and implements EarlyAwake()
			ActionBeforeAwake?.Invoke(go);
			actionBeforeAwake?.Invoke(go);
			go.SetActive(true);
			return go;
		}

		public GameObject Value => prefab.Value;

		public static implicit operator GameObject(LazyPrefab lazyPrefab) => lazyPrefab.Value;
	}
}