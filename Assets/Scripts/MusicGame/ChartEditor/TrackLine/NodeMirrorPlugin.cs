#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeMirrorPlugin : HierarchySystem<NodeMirrorPlugin>
	{
		// Serializable and Public
		[SerializeField] private SequencePriority chartEditPriority = default!;
		[SerializeField] private SequencePriority copyPasteModuleId = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Mirror", "mirror", chartEditPriority.Value, NodeMirror)
		};

		// Private
		[Inject] private CommandManager commandManager = default!;
		[Inject] private EdgeNodeSelectDataset edgeNodeSelectDataset = default!;
		[Inject] private DirectNodeSelectDataset directNodeSelectDataset = default!;
		[Inject] private ModuleInfo moduleInfo = default!;
		[Inject] private IDataset<NodeRawInfo> previewDataset = default!;

		// Event Handlers
		private bool NodeMirror()
		{
			if (moduleInfo.CurrentModule == copyPasteModuleId)
			{
				foreach (var data in previewDataset)
				{
					if (data.Type.Value is NodeType.Width) continue;
					data.Node.Value.Position = -data.Node.Value.Position;
					data.Type.Value = data.Type.Value switch
					{
						NodeType.Left => NodeType.Right,
						NodeType.Right => NodeType.Left,
						_ => data.Type.Value
					};
					data.Type.AddUpNotify();
				}

				return false;
			}

			if (edgeNodeSelectDataset.Count == 0 && directNodeSelectDataset.Count == 0) return true;

			// Mirror selected nodes
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var group in edgeNodeSelectDataset.GroupBy(node => (node.Locator.Track, node.Locator.Time)))
			{
				var groupNodes = group.ToList();
				for (var i = 0; i < groupNodes.Count; i++)
				{
					var node = groupNodes[i];
					var item = node.Model;
					var newItem = item.SetPosition(-item.Position);
					var time = node.Locator.Time;

					// Swap the sides of node pairs that share the same track and time.
					var isLeft = node.Locator.IsLeft;
					if (i % 2 == 0 && i + 1 < groupNodes.Count)
					{
						isLeft = groupNodes[i + 1].Locator.IsLeft;
					}
					else if (i % 2 == 1)
					{
						isLeft = groupNodes[i - 1].Locator.IsLeft;
					}

					var arg = new UpdateMoveListArg(isLeft, time, new(time, newItem));
					var track = node.Locator.Track;
					if (!args.ContainsKey(track)) args[track] = new() { arg };
					else args[track].Add(arg);
				}
			}

			foreach (var node in directNodeSelectDataset)
			{
				if (!node.Locator.IsPos) continue;
				var item = node.Model;
				var newItem = item.SetPosition(-item.Position);
				var time = node.Locator.Time;
				var arg = new UpdateMoveListArg(node.Locator.IsPos, time, new(time, newItem));
				var track = node.Locator.Track;
				if (!args.ContainsKey(track)) args[track] = new() { arg };
				else args[track].Add(arg);
			}

			List<ICommand> commands = (
					from pair in args
					let command = new UpdateMoveListCommand(pair.Value)
					where command.SetInit(pair.Key)
					select command)
				.Cast<ICommand>().ToList();
			commandManager.Add(new BatchCommand(commands, "Mirror Nodes"));
			return false;
		}
	}
}