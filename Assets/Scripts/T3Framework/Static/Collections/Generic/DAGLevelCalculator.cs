#nullable enable

using System;
using System.Collections.Generic;

namespace T3Framework.Static.Collections.Generic
{
	// AI-generated
	public class DAGLevelCalculator<T>
	{
		private class NodeInfo
		{
			public int Level { get; set; } = -1;
			public HashSet<T> IncomingEdges { get; } = new();
			public HashSet<T> OutgoingEdges { get; } = new();
		}

		private readonly Comparison<T> comparison;
		private readonly Dictionary<T, NodeInfo> nodeInfos;

		public ICollection<T> Nodes => nodeInfos.Keys;

		public DAGLevelCalculator(Comparison<T> comparison)
		{
			this.comparison = comparison;
			nodeInfos = new Dictionary<T, NodeInfo>();
		}

		public bool Add(T node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (nodeInfos.ContainsKey(node)) return false;

			var newNodeInfo = new NodeInfo();
			nodeInfos.Add(node, newNodeInfo);

			// Determine edges based on comparison with existing nodes
			List<T> nodesLessThan = new List<T>();
			List<T> nodesGreaterThan = new List<T>();

			foreach (var key in nodeInfos.Keys)
			{
				if (key!.Equals(node)) continue;

				int comparisonResult = comparison(key, node);
				if (comparisonResult < 0)
				{
					// key < node, so edge from key to node
					nodesLessThan.Add(key);
				}
				else if (comparisonResult > 0)
				{
					// key > node, so edge from node to key
					nodesGreaterThan.Add(key);
				}
				// If comparisonResult == 0, no edge is created
			}

			// Temporarily add edges
			foreach (var predecessor in nodesLessThan)
			{
				nodeInfos[predecessor].OutgoingEdges.Add(node);
				newNodeInfo.IncomingEdges.Add(predecessor);
			}

			foreach (var successor in nodesGreaterThan)
			{
				newNodeInfo.OutgoingEdges.Add(successor);
				nodeInfos[successor].IncomingEdges.Add(node);
			}

			// Check for cycles starting from the new node
			if (HasCycleThroughNode(node))
			{
				// Remove the node if it creates a cycle
				nodeInfos.Remove(node);

				// Clean up edges
				foreach (var predecessor in nodesLessThan)
				{
					nodeInfos[predecessor].OutgoingEdges.Remove(node);
				}

				foreach (var successor in nodesGreaterThan)
				{
					nodeInfos[successor].IncomingEdges.Remove(node);
				}

				return false;
			}

			// Incrementally update levels starting from the new node
			IncrementalUpdate(new[] { node });

			return true;
		}

		public int GetLevel(T node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (nodeInfos.TryGetValue(node, out var nodeInfo))
			{
				return nodeInfo.Level;
			}

			return -1;
		}

		private bool HasCycleThroughNode(T startNode)
		{
			HashSet<T> visited = new HashSet<T>();
			HashSet<T> recursionStack = new HashSet<T>();

			return HasCycleFromNode(startNode, visited, recursionStack);
		}

		private bool HasCycleFromNode(T node, HashSet<T> visited, HashSet<T> recursionStack)
		{
			if (recursionStack.Contains(node)) return true;
			if (!visited.Add(node)) return false;

			recursionStack.Add(node);

			if (nodeInfos.TryGetValue(node, out var nodeInfo))
			{
				foreach (var successor in nodeInfo.OutgoingEdges)
				{
					if (HasCycleFromNode(successor, visited, recursionStack))
						return true;
				}
			}

			recursionStack.Remove(node);
			return false;
		}

		private void IncrementalUpdate(IEnumerable<T> nodes)
		{
			// Only update nodes reachable from the nodes in the collection
			Queue<T> queue = new Queue<T>(nodes);
			HashSet<T> visited = new HashSet<T>();

			while (queue.Count > 0)
			{
				T current = queue.Dequeue();
				if (!visited.Add(current)) continue;

				// Calculate new level based on predecessors
				int maxPredLevel = -1;
				foreach (var pred in nodeInfos[current].IncomingEdges)
				{
					maxPredLevel = Math.Max(maxPredLevel, nodeInfos[pred].Level);
				}

				int newLevel = maxPredLevel + 1;

				// Only update if level actually changes
				if (newLevel != nodeInfos[current].Level)
				{
					nodeInfos[current].Level = newLevel;

					// Enqueue successors for potential updates
					foreach (var succ in nodeInfos[current].OutgoingEdges)
					{
						if (!visited.Contains(succ))
							queue.Enqueue(succ);
					}
				}
			}
		}

		public bool Remove(T node)
		{
			if (node == null) throw new ArgumentNullException(nameof(node));
			if (!nodeInfos.TryGetValue(node, out var info)) return false;
			// Collect affected nodes that might need level updates (successors of the node being removed)
			var affectedSuccessors = new HashSet<T>(info.OutgoingEdges);
			// Remove the node and clean up all edges
			foreach (var predecessor in nodeInfos[node].IncomingEdges)
			{
				nodeInfos[predecessor].OutgoingEdges.Remove(node);
			}

			foreach (var successor in nodeInfos[node].OutgoingEdges)
			{
				nodeInfos[successor].IncomingEdges.Remove(node);
			}

			// Now remove the node from the graph
			nodeInfos.Remove(node);
			// Incrementally update levels of affected successors
			IncrementalUpdate(affectedSuccessors);

			return true;
		}

		public void Clear() => nodeInfos.Clear();
	}
}