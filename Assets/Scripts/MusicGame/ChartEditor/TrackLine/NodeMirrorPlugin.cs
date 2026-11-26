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

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Mirror", "mirror", chartEditPriority.Value, NodeMirror)
		};

		// Private
		[Inject] private readonly CommandManager commandManager = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] private readonly IDataset<NodeRawInfo> previewDataset = default!;

		// Event Handlers
		private bool NodeMirror()
		{
			if (moduleInfo.CurrentModule == copyPasteModuleId)
			{
				foreach (var data in previewDataset)
				{
					if (data.Type.Value is TrackNodeType.Width) continue;
					data.Node.Value.Position = -data.Node.Value.Position;
					data.Type.Value = data.Type.Value switch
					{
						TrackNodeType.Left => TrackNodeType.Right,
						TrackNodeType.Right => TrackNodeType.Left,
						_ => data.Type.Value
					};
					data.Type.AddUpNotify();
				}

				return false;
			}

			if (nodeSelectDataset.Count == 0) return true;

			// Mirror selected nodes
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var group in nodeSelectDataset
				         .Where(node => node.IsEdge)
				         .GroupBy(node => (node.Track, Time: node.Time.Value)))
			{
				var groupNodes = group.ToList();
				for (var i = 0; i < groupNodes.Count; i++)
				{
					var node = groupNodes[i];
					var item = node.Model;
					var newItem = item.SetPosition(-item.Position);
					var time = node.Time.Value;

					// Swap the sides of node pairs that share the same track and time.
					var isFirst = node.IsFirst;
					if (i % 2 == 0 && i + 1 < groupNodes.Count)
					{
						isFirst = groupNodes[i + 1].IsFirst;
					}
					else if (i % 2 == 1)
					{
						isFirst = groupNodes[i - 1].IsFirst;
					}

					var arg = new UpdateMoveListArg(isFirst, time, new(time, newItem));
					AddArg(node.Track, arg);
				}
			}

			foreach (var node in nodeSelectDataset.Where(node => node.Type == TrackNodeType.Pos))
			{
				var item = node.Model;
				var newItem = item.SetPosition(-item.Position);
				var time = node.Time.Value;
				var arg = new UpdateMoveListArg(node.IsFirst, time, new(time, newItem));
				AddArg(node.Track, arg);
			}

			List<ICommand> commands = (
					from pair in args
					let command = new UpdateMoveListCommand(pair.Value)
					where command.SetInit(pair.Key)
					select command)
				.Cast<ICommand>().ToList();
			commandManager.Add(new BatchCommand(commands, "Mirror Nodes"));
			return false;

			void AddArg(ChartComponent track, UpdateMoveListArg arg)
			{
				if (!args.TryGetValue(track, out var list))
				{
					list = new();
					args[track] = list;
				}

				list.Add(arg);
			}
		}
	}
}
