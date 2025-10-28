#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace T3Framework.Runtime.Event
{
	public class EventTags : ISet<string>, IDisposable
	{
		private static string GetTagEventName(string tag) => $"EventTags_{tag}";

		private readonly HashSet<string> tags;
		private readonly GameObject gameObject;

		public EventTags(GameObject targetGameObject, IEnumerable<string>? tags = null)
		{
			gameObject = targetGameObject;
			this.tags = new HashSet<string>(tags ?? Enumerable.Empty<string>());
			RegisterAllTags();
		}

		private void DoAction(Action<GameObject> action)
		{
			if (gameObject == null) return;
			action.Invoke(gameObject);
		}

		private void RegisterTag(string tag)
		{
			var eventName = GetTagEventName(tag);
			EventManager.Instance.AddListener<Action<GameObject>>(eventName, DoAction);
		}

		private void UnregisterTag(string tag)
		{
			var eventName = GetTagEventName(tag);
			EventManager.Instance.RemoveListener<Action<GameObject>>(eventName, DoAction);
		}

		private void RegisterAllTags()
		{
			foreach (var eventName in tags.Select(GetTagEventName))
			{
				EventManager.Instance.AddListener<Action<GameObject>>(eventName, DoAction);
			}
		}

		private void UnregisterAllTags()
		{
			foreach (var eventName in tags.Select(GetTagEventName))
			{
				EventManager.Instance.RemoveListener<Action<GameObject>>(eventName, DoAction);
			}
		}

		public void Dispose() => UnregisterAllTags();

		// Implement ISet<string>
		public IEnumerator<string> GetEnumerator()
		{
			return tags.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		void ICollection<string>.Add(string item)
		{
			if (tags.Add(item)) RegisterTag(item);
		}

		public void ExceptWith(IEnumerable<string> other)
		{
			UnregisterAllTags();
			tags.ExceptWith(other);
			RegisterAllTags();
		}

		public void IntersectWith(IEnumerable<string> other)
		{
			UnregisterAllTags();
			tags.IntersectWith(other);
			RegisterAllTags();
		}

		public bool IsProperSubsetOf(IEnumerable<string> other) => tags.IsProperSubsetOf(other);

		public bool IsProperSupersetOf(IEnumerable<string> other) => tags.IsProperSupersetOf(other);

		public bool IsSubsetOf(IEnumerable<string> other) => tags.IsSubsetOf(other);

		public bool IsSupersetOf(IEnumerable<string> other) => tags.IsSupersetOf(other);

		public bool Overlaps(IEnumerable<string> other) => tags.Overlaps(other);

		public bool SetEquals(IEnumerable<string> other) => tags.SetEquals(other);

		public void SymmetricExceptWith(IEnumerable<string> other)
		{
			UnregisterAllTags();
			tags.SymmetricExceptWith(other);
			RegisterAllTags();
		}

		public void UnionWith(IEnumerable<string> other)
		{
			UnregisterAllTags();
			tags.UnionWith(other);
			RegisterAllTags();
		}

		bool ISet<string>.Add(string item)
		{
			if (tags.Add(item))
			{
				RegisterTag(item);
				return true;
			}

			return false;
		}

		public void Clear()
		{
			UnregisterAllTags();
			tags.Clear();
		}

		public bool Contains(string item) => tags.Contains(item);

		public void CopyTo(string[] array, int arrayIndex) => tags.CopyTo(array, arrayIndex);

		public bool Remove(string item)
		{
			if (tags.Remove(item))
			{
				UnregisterTag(item);
				return true;
			}

			return false;
		}

		public int Count => tags.Count;

		public bool IsReadOnly => false;
	}
}