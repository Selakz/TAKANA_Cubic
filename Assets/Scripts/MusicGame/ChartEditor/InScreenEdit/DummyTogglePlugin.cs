#nullable enable

using System.Collections.Generic;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class DummyTogglePlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "ToggleDummy", () =>
			{
				List<ICommand> commands = new List<ICommand>();
				foreach (var component in dataset)
				{
					if (component.Model is INote note)
					{
						var isDummy = note.IsDummy();
						ICommand command = new UpdateComponentCommand
							(component, _ => note.SetDummy(!isDummy), _ => note.SetDummy(isDummy));
						commands.Add(command);
					}
				}

				CommandManager.Instance.Add(new BatchCommand(commands, "ToggleDummy"));
			})
		};

		// Private
		private ChartSelectDataset dataset = default!;

		// Defined Functions
		[Inject]
		private void Construct(ChartSelectDataset dataset) => this.dataset = dataset;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}