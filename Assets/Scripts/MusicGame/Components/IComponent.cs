using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.MVC;

namespace MusicGame.Components
{
	/// <summary>
	/// Representing components in playfield.
	/// </summary>
	public interface IComponent : IModel, ISerializable
	{
		// TODO (Another Huge TODO): Remove this and replace with TimeStart { get; set; }
		// Note: TimeStart means the latest valid time when the component appear, and TimeEnd means the earliest valid time when the component disappear.
		public T3Time TimeInstantiate { get; }

		public T3Time TimeEnd { get; set; }

		public bool IsPresent { get; }

		/// <summary> The parent component. Null means it's a root component. </summary>
		public IComponent Parent { get; set; }

		public JObject Properties { get; }

		private static int maxId = 0;
		public static int GetUniqueId() => maxId++;
		public static void ResetId() => maxId = 0;
		public static void SetIdMinValue(int id) => maxId = Math.Max(maxId, id);

		/// <summary> Topologically sort the given components by their parents. </summary>
		public static List<IComponent> TopologicalSort(List<IComponent> components)
		{
			var graph = new Dictionary<IComponent, HashSet<IComponent>>();
			var inDegree = new Dictionary<IComponent, int>();
			foreach (var component in components)
			{
				graph[component] = new HashSet<IComponent>();
				inDegree[component] = 0;
			}

			foreach (var component in components)
			{
				var parent = component.Parent;
				if (parent != null && graph.ContainsKey(parent))
				{
					graph[parent].Add(component);
					inDegree[component]++;
				}
			}

			var queue = new Queue<IComponent>();
			foreach (var comp in components.Where(comp => inDegree[comp] == 0))
			{
				queue.Enqueue(comp);
			}

			var result = new List<IComponent>();
			while (queue.Count > 0)
			{
				var current = queue.Dequeue();
				result.Add(current);

				foreach (var child in graph[current])
				{
					inDegree[child]--;
					if (inDegree[child] == 0)
					{
						queue.Enqueue(child);
					}
				}
			}

			if (result.Count != components.Count)
			{
				throw new InvalidOperationException("In topological sort: Circular dependency detected.");
			}

			return result;
		}
	}

	public interface IChildOf<T> where T : IComponent
	{
		public T Parent { get; set; }
	}
}