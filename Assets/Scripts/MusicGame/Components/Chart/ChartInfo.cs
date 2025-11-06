#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace MusicGame.Components.Chart
{
	public class ChartInfo : IEnumerable<IComponent>, ISerializable
	{
		public static string TypeMark => "chart";

		public JObject Properties { get; set; }

		public IComponent? this[int id] => componentDict.ContainsKey(id) ? componentDict[id] : null;

		private readonly Dictionary<int, IComponent> componentDict;
		private readonly SortedDictionary<IComponent, int> components;

		public ChartInfo()
		{
			Properties = new();
			componentDict = new();
			components = new(Comparer<IComponent>.Create(
				(x, y) => (x.TimeInstantiate, x.Id).CompareTo((y.TimeInstantiate, y.Id))));
		}

		public ChartInfo(IEnumerable<IComponent> components)
			: this()
		{
			foreach (var component in components)
			{
				AddComponent(component);
			}
		}

		public ChartInfo(T3Time offset, IEnumerable<IComponent> components)
			: this(components)
		{
			Properties.Add("offset", offset.ToString());
		}

		public bool TryGetComponent(int id, out IComponent component)
		{
			return componentDict.TryGetValue(id, out component);
		}

		public bool Contains(int id)
		{
			return componentDict.ContainsKey(id);
		}

		public bool Contains(IComponent component)
		{
			return components.ContainsKey(component);
		}

		public IComponent? AddComponent(IComponent component)
		{
			IComponent? previous = null;
			if (componentDict.TryGetValue(component.Id, out var value))
			{
				previous = value;
			}

			componentDict[component.Id] = component;
			if (previous is not null) components.Remove(previous);
			components[component] = component.Id;
			return previous;
		}

		public bool RemoveComponent(int id)
		{
			if (componentDict.ContainsKey(id))
			{
				components.Remove(componentDict[id]);
				componentDict.Remove(id);
				return true;
			}

			return false;
		}

		public IEnumerator<IComponent> GetEnumerator()
		{
			return components.Keys.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public JToken GetSerializationToken()
		{
			var list = new JArray();
			var sortedComponents = IComponent.TopologicalSort(componentDict.Values);
			foreach (var component in sortedComponents)
			{
				list.Add(component.Serialize(true));
			}

			var token = new JObject
			{
				["properties"] = Properties,
				["components"] = list
			};
			return token;
		}

		public static ChartInfo Deserialize(JToken token)
		{
			IComponent.ResetId();
			if (token is not JContainer container) return new(Array.Empty<IComponent>());
			JObject properties = container.TryGetValue("properties", out JObject propsToken) ? propsToken : new();
			List<IComponent> components = new();
			int maxId = 0;
			foreach (var componentToken in token["components"]!)
			{
				var component = (IComponent)ISerializable.Deserialize(componentToken, components);
				components.Add(component);
				maxId = Math.Max(maxId, component.Id);
			}

			IComponent.SetIdMinValue(maxId + 1);
			return new(components)
			{
				Properties = properties
			};
		}
	}
}