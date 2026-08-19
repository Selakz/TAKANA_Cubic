#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using EditorPlugin.Shared.To;
using MusicGame.ChartEditor.Decoration.Track;
using T3Framework.Runtime.Log;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared
{
	public class NodeApi : IDisposable
	{
		private readonly EdgeNodeDataset edgeDataset;
		private readonly DirectNodeDataset directDataset;
		private readonly EdgeNodeSelectDataset edgeSelectDataset;
		private readonly DirectNodeSelectDataset directSelectDataset;
		private readonly ChartApi api;

		private readonly Dictionary<EdgeNodeComponent, RawNodeData> edgeNodes = new();
		private readonly Dictionary<DirectNodeComponent, RawNodeData> directNodes = new();

		private Action<object>? onNodeAddedInner;
		private Action<object>? onNodeRemovedInner;

		internal NodeApi(
			EdgeNodeDataset edgeDataset,
			DirectNodeDataset directDataset,
			EdgeNodeSelectDataset edgeSelectDataset,
			DirectNodeSelectDataset directSelectDataset,
			ChartApi api)
		{
			this.edgeDataset = edgeDataset;
			this.directDataset = directDataset;
			this.edgeSelectDataset = edgeSelectDataset;
			this.directSelectDataset = directSelectDataset;
			this.api = api;

			edgeDataset.OnDataAdded += OnNodeAdded;
			edgeDataset.BeforeDataRemoved += OnNodeRemoved;
			directDataset.OnDataAdded += OnNodeAdded;
			directDataset.BeforeDataRemoved += OnNodeRemoved;

			foreach (var node in edgeDataset) OnNodeAdded(node);
			foreach (var node in directDataset) OnNodeAdded(node);
		}

		public void onNodeAdded(Action<object> callback) => onNodeAddedInner += callback;

		public void onNodeRemoved(Action<object> callback) => onNodeRemovedInner += callback;

		public object[] getAllNodes()
		{
			return edgeNodes.Values.Concat(directNodes.Values).Cast<object>().ToArray();
		}

		public object[] getAllSelected()
		{
			var list = new List<object>();
			foreach (var component in edgeSelectDataset)
			{
				if (edgeNodes.TryGetValue(component, out var raw)) list.Add(raw);
			}

			foreach (var component in directSelectDataset)
			{
				if (directNodes.TryGetValue(component, out var raw)) list.Add(raw);
			}

			return list.ToArray();
		}

		public object? getCurrentSelecting()
		{
			if (edgeSelectDataset.CurrentSelecting.Value is { } edge &&
			    edgeNodes.TryGetValue(edge, out var edgeRaw))
			{
				return edgeRaw;
			}

			if (directSelectDataset.CurrentSelecting.Value is { } direct &&
			    directNodes.TryGetValue(direct, out var directRaw))
			{
				return directRaw;
			}

			return null;
		}

		public void addSelected(object raw)
		{
			if (raw is not RawNodeData nodeData) return;
			if (nodeData.EdgeComponent is { } edge) edgeSelectDataset.Add(edge);
			else if (nodeData.DirectComponent is { } direct) directSelectDataset.Add(direct);
		}

		public void removeSelected(object raw)
		{
			if (raw is not RawNodeData nodeData) return;
			if (nodeData.EdgeComponent is { } edge) edgeSelectDataset.Remove(edge);
			else if (nodeData.DirectComponent is { } direct) directSelectDataset.Remove(direct);
		}

		public void clearSelected()
		{
			edgeSelectDataset.Clear();
			directSelectDataset.Clear();
		}

		private void OnNodeAdded(EdgeNodeComponent component)
		{
			var raw = new RawNodeData(component, api);
			edgeNodes[component] = raw;
			try
			{
				onNodeAddedInner?.Invoke(raw);
			}
			catch
			{
				T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNodeAdded", T3LogType.Error);
			}
		}

		private void OnNodeRemoved(EdgeNodeComponent component)
		{
			if (edgeNodes.Remove(component, out var raw))
			{
				try
				{
					onNodeRemovedInner?.Invoke(raw);
				}
				catch
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNodeRemoved", T3LogType.Error);
				}
			}
		}

		private void OnNodeAdded(DirectNodeComponent component)
		{
			var raw = new RawNodeData(component, api);
			directNodes[component] = raw;
			try
			{
				onNodeAddedInner?.Invoke(raw);
			}
			catch
			{
				T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNodeAdded", T3LogType.Error);
			}
		}

		private void OnNodeRemoved(DirectNodeComponent component)
		{
			if (directNodes.Remove(component, out var raw))
			{
				try
				{
					onNodeRemovedInner?.Invoke(raw);
				}
				catch
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNodeRemoved", T3LogType.Error);
				}
			}
		}

		public void Dispose()
		{
			edgeDataset.OnDataAdded -= OnNodeAdded;
			edgeDataset.BeforeDataRemoved -= OnNodeRemoved;
			directDataset.OnDataAdded -= OnNodeAdded;
			directDataset.BeforeDataRemoved -= OnNodeRemoved;
			onNodeAddedInner = null;
			onNodeRemovedInner = null;
			edgeNodes.Clear();
			directNodes.Clear();
		}
	}
}