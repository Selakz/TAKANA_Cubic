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
		private readonly TrackNodeDataset nodeDataset;
		private readonly TrackNodeSelectDataset nodeSelectDataset;
		private readonly ChartApi api;

		private readonly Dictionary<TrackNodeComponent, RawNodeData> nodes = new();

		private Action<object>? onNodeAddedInner;
		private Action<object>? onNodeRemovedInner;

		internal NodeApi(
			TrackNodeDataset nodeDataset,
			TrackNodeSelectDataset nodeSelectDataset,
			ChartApi api)
		{
			this.nodeDataset = nodeDataset;
			this.nodeSelectDataset = nodeSelectDataset;
			this.api = api;

			nodeDataset.OnDataAdded += OnNodeAdded;
			nodeDataset.BeforeDataRemoved += OnNodeRemoved;

			foreach (var node in nodeDataset) OnNodeAdded(node);
		}

		public void onNodeAdded(Action<object> callback) => onNodeAddedInner += callback;

		public void onNodeRemoved(Action<object> callback) => onNodeRemovedInner += callback;

		public object[] getAllNodes() => nodes.Values.Cast<object>().ToArray();

		public object[] getAllSelected()
		{
			List<object> list = new();
			foreach (var component in nodeSelectDataset)
			{
				if (nodes.TryGetValue(component, out var raw)) list.Add(raw);
			}

			return list.ToArray();
		}

		public object? getCurrentSelecting()
		{
			if (nodeSelectDataset.CurrentSelecting.Value is { } selecting &&
			    nodes.TryGetValue(selecting, out var raw))
			{
				return raw;
			}

			return null;
		}

		public void addSelected(object raw)
		{
			if (raw is not RawNodeData nodeData) return;
			nodeSelectDataset.Add(nodeData.Component);
		}

		public void removeSelected(object raw)
		{
			if (raw is not RawNodeData nodeData) return;
			nodeSelectDataset.Remove(nodeData.Component);
		}

		public void clearSelected() => nodeSelectDataset.Clear();

		private void OnNodeAdded(TrackNodeComponent component)
		{
			var raw = new RawNodeData(component, api);
			nodes[component] = raw;
			try
			{
				onNodeAddedInner?.Invoke(raw);
			}
			catch (Exception e)
			{
				T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNodeAdded", T3LogType.Error);
				T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
			}
		}

		private void OnNodeRemoved(TrackNodeComponent component)
		{
			if (nodes.Remove(component, out var raw))
			{
				try
				{
					onNodeRemovedInner?.Invoke(raw);
				}
				catch (Exception e)
				{
					T3Logger.Log("Notice", "EditorPlugin_PluginInternalError|onNodeRemoved", T3LogType.Error);
					T3Logger.Log("MessageRaw", $"{e.Message}\n{e.StackTrace}");
				}
			}
		}

		public void Dispose()
		{
			nodeDataset.OnDataAdded -= OnNodeAdded;
			nodeDataset.BeforeDataRemoved -= OnNodeRemoved;
			onNodeAddedInner = null;
			onNodeRemovedInner = null;
			nodes.Clear();
		}
	}
}
