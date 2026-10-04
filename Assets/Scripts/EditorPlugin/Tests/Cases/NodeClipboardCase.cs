#nullable enable

using System.Collections.Generic;
using System.Linq;
using EditorPlugin.Shared;
using EditorPlugin.Shared.To;
using MusicGame.ChartEditor.TrackLine;
using MusicGame.Models.Track.Movement;
using T3Framework.Runtime;
using T3Framework.Static.Easing;

namespace EditorPlugin.Tests.Cases
{
	/// <summary>
	/// C# side checks of the scenarios declared by
	/// <c>Assets/StreamingAssets/Excluded/EditorPluginTests/cases/nodeClipboard.ts</c>.
	/// They assert the clipboard content the plugin left behind, and the group filter of the read path.
	/// </summary>
	public static class NodeClipboardCase
	{
		[PluginTestCheck("nodeClipboard.override-and-read")]
		public static void OverrideAndRead(PluginTestCheckContext ctx)
		{
			var nodes = ctx.NodeClipboardItems.ToList();
			ctx.Check(nodes.Count == 4, "the clipboard holds the four nodes the plugin wrote");
			ctx.Check(nodes.All(node => node.Type.Value is NodeType.Left or NodeType.Right),
				"the clipboard holds edge nodes only");
			ctx.Check(nodes.All(node => node.Parent.Value == NodeClipboardApi.Stub),
				"every node points at the stub component");
			ctx.Check(NodeClipboardApi.Stub.BelongingChart is null && NodeClipboardApi.Stub.Parent is null,
				"the stub component belongs to no chart");
			ctx.Check(nodes.Where(node => node.Type.Value == NodeType.Left)
					.Select(node => node.Time.Value.Milli).OrderBy(milli => milli).SequenceEqual(new[] { 100, 300 }),
				"the clipboard holds the two left nodes");
			ctx.Check(nodes.Count(node => node.Type.Value == NodeType.Right) == 2,
				"the clipboard holds the two right nodes");
			ctx.Check(Find(nodes, NodeType.Left, 100)?.Node.Value is V1EMoveItem { Position: -2, Ease: Eases.Linear },
				"the left node at 100 kept its position and ease");
			ctx.Check(Find(nodes, NodeType.Left, 300)?.Node.Value is V1BMoveItem { Position: -4 },
				"the left node at 300 is still a bezier node");
			ctx.Check(Find(nodes, NodeType.Right, 100)?.Node.Value is V1EMoveItem { Position: 2, Ease: Eases.Unmove },
				"the right node at 100 kept its position and ease");
			ctx.Check(Find(nodes, NodeType.Right, 300)?.Node.Value is V1EMoveItem { Position: 4 },
				"the right node at 300 kept its position");
			ctx.Check(ctx.Commands.BatchCount == 0, "the clipboard writes produced no command");

			// The editor allows both groups in one clipboard; the read hands out the edge group then. The api is
			// built around a temporary list, so the real clipboard of the scenario stays as the plugin left it.
			using var mixed = new NodeClipboardApi(new List<NodeRawInfo>
			{
				new(new T3Time(100), NodeType.Left, new V1EMoveItem(-2, Eases.Linear), NodeClipboardApi.Stub),
				new(new T3Time(100), NodeType.Pos, new V1EMoveItem(0, Eases.Unmove), NodeClipboardApi.Stub),
				new(new T3Time(100), NodeType.Width, new V1EMoveItem(4, Eases.Unmove), NodeClipboardApi.Stub)
			});
			var mixedNodes = mixed.readNodes();
			ctx.Check(mixedNodes.Length == 1 && mixedNodes[0] is RawClipboardNodeData { type: "Left" },
				"a mixed clipboard hands out its edge nodes only");

			using var direct = new NodeClipboardApi(new List<NodeRawInfo>
			{
				new(new T3Time(200), NodeType.Pos, new V1EMoveItem(1, Eases.Linear), NodeClipboardApi.Stub),
				new(new T3Time(200), NodeType.Width, new V1EMoveItem(4, Eases.Linear), NodeClipboardApi.Stub)
			});
			var directNodes = direct.readNodes();
			ctx.Check(directNodes.Length == 2 && directNodes.All(raw => raw is RawClipboardNodeData),
				"a direct clipboard hands out both its pos and its width node");

			static NodeRawInfo? Find(List<NodeRawInfo> nodes, NodeType type, int milli) => nodes.SingleOrDefault(
				node => node.Type.Value == type && node.Time.Value.Milli == milli);
		}
	}
}
