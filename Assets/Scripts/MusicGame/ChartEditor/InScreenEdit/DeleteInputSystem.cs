#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Select;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class DeleteInputSystem : HierarchySystem<DeleteInputSystem>
	{
		// Serializable and Public
		[SerializeField] private SequencePriorities moduleIds = default!;

		// Event Registrars
		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(moduleInfo.CurrentModule, id => IsEnabled = moduleIds.Values.Any(x => x == id))
		};

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Delete",
				() =>
				{
					var command = system.DeleteSelected();
					if (command is not null) commandManager.Add(command);
				}),
		};

		// Private
		[Inject] private readonly ModuleInfo moduleInfo = default!;
		[Inject] private readonly CommandManager commandManager = default!;
		[Inject] private readonly ChartEditSystem system = default!;
		[Inject] private readonly ChartSelectDataset dataset = default!;
	}
}