#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace T3Framework.Runtime.ECS
{
	/// <summary>
	/// Manage view plugins. <see cref="PrefabObject.Instantiate"/> will automatically add this script to the prefab.
	/// </summary>
	public class PrefabHandler : MonoBehaviour, IEnumerable<KeyValuePair<string, PrefabHandler>>
	{
		private readonly Dictionary<Type, object> scripts = new();
		private readonly Dictionary<string, PrefabHandler> plugins = new();

		public event Action<string>? OnPluginAdded;
		public event Action<string>? BeforePluginRemoved;

		public PrefabHandler? Parent { get; private set; }

		public PrefabHandler? this[string id] => GetPlugin(id);

		public T Script<T>()
		{
			var type = typeof(T);
			if (scripts.TryGetValue(type, out var script)) return (T)script;
			else
			{
				var component = gameObject.GetComponent<T>();
				if (component is null) throw new NullReferenceException($"the prefab do not contain component {type}");
				scripts[type] = component;
				return component;
			}
		}

		public T? TryScript<T>()
		{
			var type = typeof(T);
			if (scripts.TryGetValue(type, out var script)) return (T)script;
			else
			{
				if (gameObject.TryGetComponent(type, out var component) && component is T t)
				{
					scripts[type] = t;
					return t;
				}

				return default;
			}
		}

		public void AddPlugin(string id, PrefabHandler plugin)
		{
			if (!plugins.TryAdd(id, plugin)) return;
			plugin.transform.SetParent(transform, false);
			plugin.Parent = this;
			plugin.gameObject.SetActive(true);
			plugin.gameObject.name = id;
			OnPluginAdded?.Invoke(id);
		}

		public void RemovePlugin(string id, Transform newParent)
		{
			if (!plugins.ContainsKey(id)) return;
			BeforePluginRemoved?.Invoke(id);
			plugins.Remove(id, out var plugin);
			plugin.Parent = null;
			plugin.gameObject.SetActive(false);
			plugin.transform.SetParent(newParent, false);
		}

		public void RemovePlugin(PrefabHandler plugin, Transform newParent)
		{
			string? id = null;
			foreach (var pair in plugins)
			{
				if (pair.Value == plugin)
				{
					id = pair.Key;
					break;
				}
			}

			if (id == null) return;
			plugins.Remove(id);
			plugin.Parent = null;
			plugin.gameObject.SetActive(false);
			plugin.transform.SetParent(newParent, false);
			BeforePluginRemoved?.Invoke(id);
		}

		public PrefabHandler? GetPlugin(params string[] ids)
		{
			var currentHandler = this;
			foreach (var id in ids)
			{
				if (currentHandler.plugins.TryGetValue(id, out var plugin)) currentHandler = plugin;
				else return null;
			}

			return currentHandler;
		}

		public IEnumerable<PrefabHandler> GetPlugins() => plugins.Values;

		public IEnumerator<KeyValuePair<string, PrefabHandler>> GetEnumerator() => plugins.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}