#nullable enable

using System;
using System.Collections.Generic;
using EditorPlugin.Shared.To;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Track;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;

namespace EditorPlugin.Shared
{
	/// <summary>
	/// The node clipboard of the editor, as seen by plugins. Reads hand out value snapshots of the nodes, so
	/// plugin writes to what they read change neither the clipboard nor the chart; the only write replaces the
	/// whole clipboard content with copies of the movement the plugin provides.
	/// </summary>
	public class NodeClipboardApi : IDisposable
	{
		// Public
		/// <summary>
		/// The component the nodes of a plugin written clipboard point at. The editor never uses it: pasting
		/// replaces it with the track the paste targets, so a detached component without a chart is enough.
		/// </summary>
		public static ChartComponent Stub { get; } = new(new Track(new T3Time(0), new T3Time(0)));

		// Private
		private readonly List<NodeRawInfo> clipboard;

		// Constructor
		public NodeClipboardApi(List<NodeRawInfo> clipboard)
		{
			this.clipboard = clipboard;
		}

		// Defined Functions
		/// <summary> Nothing to release: the api holds no resource of its own. </summary>
		public void Dispose()
		{
		}

		/// <summary>
		/// One value snapshot per clipboard node, in clipboard order. The clipboard may mix edge nodes
		/// (Left/Right) with direct nodes (Pos/Width); only one group is handed out, and the edge group wins
		/// because a paste targets one kind of movement either. Empty when the clipboard is empty.
		/// </summary>
		public object[] readNodes()
		{
			List<NodeRawInfo> edge = new();
			List<NodeRawInfo> direct = new();
			foreach (var info in clipboard)
			{
				if (info.Type.Value is NodeType.Left or NodeType.Right) edge.Add(info);
				else direct.Add(info);
			}

			var nodes = edge.Count > 0 ? edge : direct;
			List<object> raws = new(nodes.Count);
			foreach (var info in nodes) raws.Add(new RawClipboardNodeData(info));
			return raws.ToArray();
		}

		/// <summary>
		/// Replaces the whole clipboard content with the nodes of the given track movement: a
		/// <see cref="TrackEdgeMovement" /> writes left/right nodes, a <see cref="TrackDirectMovement" /> writes
		/// position/width nodes. Any other movement is rejected, leaving the clipboard untouched.
		/// </summary>
		public bool overrideMovement(object movement)
		{
			if (movement is not ITrackMovement trackMovement) return false;
			List<NodeRawInfo> nodes = new();
			switch (trackMovement)
			{
				case TrackEdgeMovement { Movement1: ChartPosMoveList left, Movement2: ChartPosMoveList right }:
					AddNodes(nodes, left, NodeType.Left);
					AddNodes(nodes, right, NodeType.Right);
					break;
				case TrackDirectMovement { Movement1: ChartPosMoveList position, Movement2: ChartPosMoveList width }:
					AddNodes(nodes, position, NodeType.Pos);
					AddNodes(nodes, width, NodeType.Width);
					break;
				default:
					return false;
			}

			clipboard.Clear();
			clipboard.AddRange(nodes);
			return true;

			// The clipboard keeps its own item copies, so a plugin that holds on to the movement it wrote cannot
			// change the clipboard afterwards.
			static void AddNodes(List<NodeRawInfo> target, ChartPosMoveList list, NodeType type)
			{
				foreach (var (time, item) in list) target.Add(new NodeRawInfo(time, type, item.Clone(), Stub));
			}
		}
	}
}
