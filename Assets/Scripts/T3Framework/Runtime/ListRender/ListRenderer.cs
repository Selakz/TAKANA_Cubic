using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace T3Framework.Runtime.ListRender
{
	/// <summary>
	/// To use this class, inherit it to form a non-generic class 
	/// </summary>
	public class ListRenderer<TKey> : MonoBehaviour, IEnumerable<KeyValuePair<TKey, GameObject>>
		where TKey : IComparable<TKey>
	{
		// Serializable and Public
		[SerializeField] private RectTransform parentTransform;

		public Comparison<TKey> ListSorter
		{
			get => listSorter;
			set
			{
				listSorter = value;
				Sort();
			}
		}

		public RectTransform ParentTransform
		{
			get => parentTransform;
			set
			{
				parentTransform = value;
				foreach (var child in Values)
				{
					child.transform.SetParent(parentTransform);
				}
			}
		}

		public bool AlwaysRebuildLayout { get; set; } = true;

		public Dictionary<TKey, GameObject>.KeyCollection Keys => listItems.Keys;

		public Dictionary<TKey, GameObject>.ValueCollection Values => listItems.Values;

		public int Count => listItems.Count;

		// Private
		private Comparison<TKey> listSorter = (a, b) => a.CompareTo(b);
		private Dictionary<Type, Lazy<ObjectPool<GameObject>>> listItemPools;
		private readonly Dictionary<TKey, GameObject> listItems = new();

		// Defined Functions
		public void Init(Dictionary<Type, LazyPrefab> listPrefabs)
		{
			listItemPools = new();
			foreach (var pair in listPrefabs)
			{
				if (!pair.Key.IsSubclassOf(typeof(MonoBehaviour)))
				{
					Debug.LogError($"{pair.Key} is not a subclass of MonoBehaviour");
					continue;
				}

				listItemPools.Add(pair.Key, new Lazy<ObjectPool<GameObject>>(
					() => new ObjectPool<GameObject>(
						() => pair.Value.Instantiate(ParentTransform),
						go => go.SetActive(true),
						go => go.SetActive(false),
						Destroy
					)));
			}
		}

		private void Sort()
		{
			List<TKey> keys = new List<TKey>(listItems.Keys);
			keys.Sort(ListSorter);
			for (int i = 0; i < keys.Count; i++)
			{
				listItems[keys[i]].transform.SetSiblingIndex(i);
			}

			if (AlwaysRebuildLayout) RebuildLayout();
		}

		public T Add<T>(TKey key) where T : MonoBehaviour
		{
			var type = typeof(T);
			if (!listItemPools.ContainsKey(type)) return null;
			var go = listItemPools[type].Value.Get();
			listItems.Add(key, go);
			Sort();
			return go.GetComponent<T>();
		}

		public MonoBehaviour Add(Type type, TKey key)
		{
			if (!listItemPools.ContainsKey(type)) return null;
			var go = listItemPools[type].Value.Get();
			listItems.Add(key, go);
			Sort();
			return (MonoBehaviour)go.GetComponent(type);
		}

		public bool TryGet<T>(TKey key, out T value) where T : MonoBehaviour
		{
			var type = typeof(T);
			if (!listItemPools.ContainsKey(type))
			{
				value = default;
				return false;
			}

			var result = listItems.TryGetValue(key, out var go);
			if (!result)
			{
				value = default;
				return false;
			}

			value = go.GetComponent<T>();
			return true;
		}

		public bool TryGet(Type type, TKey key, out MonoBehaviour value)
		{
			if (!listItemPools.ContainsKey(type))
			{
				value = default;
				return false;
			}

			var result = listItems.TryGetValue(key, out var go);
			if (!result)
			{
				value = default;
				return false;
			}

			value = (MonoBehaviour)go.GetComponent(type);
			return true;
		}

		public void Remove(TKey key)
		{
			if (!listItems.Remove(key, out var go)) return;
			foreach (var pair in listItemPools)
			{
				if (go.GetComponent(pair.Key) != null)
				{
					pair.Value.Value.Release(go);
					if (AlwaysRebuildLayout) RebuildLayout();
					return;
				}
			}
		}

		public void RebuildLayout()
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(ParentTransform);
		}

		public int GetSiblingIndex(TKey key)
		{
			if (!listItems.ContainsKey(key)) return -1;
			if (!listItems.TryGetValue(key, out var go)) return -1;
			return go.transform.GetSiblingIndex();
		}

		/// <summary> Swap the item of the given key with its previous item. </summary>
		public bool SwapUp(TKey key, out TKey swappedKey)
		{
			if (!listItems.Remove(key, out var go))
			{
				swappedKey = default;
				return false;
			}

			List<TKey> keys = new List<TKey>(listItems.Keys);
			keys.Sort(ListSorter);
			keys.Reverse();
			foreach (var k in keys)
			{
				if (ListSorter.Invoke(k, key) < 0)
				{
					swappedKey = k;
					if (listItems.Remove(swappedKey, out var swappedObject))
					{
						listItems[key] = swappedObject;
						listItems[swappedKey] = go;
					}

					Sort();
					return true;
				}
			}

			listItems[key] = go;
			swappedKey = default;
			return false;
		}

		/// <summary> Swap the item of the given key with its following item. </summary>
		public bool SwapDown(TKey key, out TKey swappedKey)
		{
			if (!listItems.Remove(key, out var go))
			{
				swappedKey = default;
				return false;
			}

			List<TKey> keys = new List<TKey>(listItems.Keys);
			keys.Sort(ListSorter);
			foreach (var k in keys)
			{
				if (ListSorter.Invoke(k, key) > 0)
				{
					swappedKey = k;
					if (listItems.Remove(swappedKey, out var swappedObject))
					{
						listItems[key] = swappedObject;
						listItems[swappedKey] = go;
					}

					Sort();
					return true;
				}
			}

			listItems[key] = go;
			swappedKey = default;
			return false;
		}

		public void Clear()
		{
			var enumerate = listItems.Keys.ToList();
			foreach (var key in enumerate)
			{
				Remove(key);
			}
		}

		public IEnumerator<KeyValuePair<TKey, GameObject>> GetEnumerator()
		{
			return listItems.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}