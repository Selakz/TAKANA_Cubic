#nullable enable

using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeMirrorPlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private SequencePriority chartEditPriority = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Mirror", "mirror", chartEditPriority.Value, NodeMirror)
		};

		// Private
		private CommandManager commandManager = default!;
		private EdgeNodeSelectDataset nodeSelectDataset = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			CommandManager commandManager,
			EdgeNodeSelectDataset nodeSelectDataset)
		{
			this.commandManager = commandManager;
			this.nodeSelectDataset = nodeSelectDataset;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private bool NodeMirror()
		{
			if (nodeSelectDataset.Count == 0) return true;
			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset)
			{
				var item = node.Model;
				var newItem = item.SetPosition(-item.Position);
				var time = node.Locator.Time;
				var arg = new UpdateMoveListArg(node.Locator.IsLeft, time, new(time, newItem));
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