#nullable enable

using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace T3Framework.Runtime.ECS
{
	[Serializable]
	public class PrefabObject
	{
		[SerializeField] private GameObject prefab;

		public GameObject Prefab => prefab;

		private bool isInitialized = false;

		public PrefabObject(GameObject prefab)
		{
			this.prefab = prefab;
			Initialize();
		}

		public void Initialize()
		{
			if (isInitialized) return;
			isInitialized = true;
			prefab.SetActive(false);
#if UNITY_EDITOR
			Application.quitting += () => prefab.SetActive(true);
#endif
		}

		public PrefabHandler Instantiate(IObjectResolver resolver, Transform parent, bool isActive = true)
		{
			Initialize();
			var go = resolver.Instantiate(Prefab, parent);
			if (!go.TryGetComponent<PrefabHandler>(out var handler)) handler = go.AddComponent<PrefabHandler>();
			go.SetActive(isActive);
			return handler;
		}
	}
}