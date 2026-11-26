#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.ChartEditor.TrackLine.Commands;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.ChartEditor.TrackLine
{
	public class NodeTimingBlockSystem : HierarchySystem<NodeTimingBlockSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<PropertyChangedEventHandler>(
				e => timingBlockClickTime.PropertyChanged += e,
				e => timingBlockClickTime.PropertyChanged -= e,
				(_, _) => OnTimingBlockClicked(timingBlockClickTime.Value))
		};

		// Private
		[Inject] private readonly NotifiableProperty<T3Time> timingBlockClickTime = default!;
		[Inject] private readonly TrackNodeSelectDataset nodeSelectDataset = default!;
		[Inject] private readonly CommandManager commandManager = default!;

		// Defined Functions
		private void OnTimingBlockClicked(T3Time time)
		{
			if (nodeSelectDataset.Count == 0) return;

			HashSet<(ChartComponent, bool)> seenMovements = new();
			foreach (var node in nodeSelectDataset)
			{
				if (!seenMovements.Add((node.Track, node.IsFirst))) return;
			}

			Dictionary<ChartComponent, List<UpdateMoveListArg>> args = new();
			foreach (var node in nodeSelectDataset)
			{
				var oldTime = node.Time.Value;
				if (oldTime == time) continue;
				if (!args.TryGetValue(node.Track, out var list))
				{
					list = new();
					args[node.Track] = list;
				}

				list.Add(new UpdateMoveListArg(node.IsFirst, oldTime, new(time, node.Model)));
			}

			List<ICommand> commands = (
					from pair in args
					let command = new UpdateMoveListCommand(pair.Value)
					where command.SetInit(pair.Key)
					select command)
				.Cast<ICommand>().ToList();
			if (commands.Count > 0) commandManager.Add(new BatchCommand(commands, "Set Node Time"));
		}
	}
}
