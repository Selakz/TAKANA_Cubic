#nullable enable

using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Log;
using T3Framework.Runtime.VContainer;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class CommandProcessSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			// Select added track
			new CommandFetchRegistrar<AddComponentCommand, ChartComponent?>(commandManager, CommandProcess.Redo,
				command => command.Component,
				tracks =>
				{
					selectDataset.Clear();
					foreach (var track in tracks) selectDataset.Add(track!);
				},
				command => command.Component?.Model is ITrack),
			// Notify deleted notes
			new CommandFetchRegistrar<DeleteComponentCommand, ChartComponent>(commandManager, CommandProcess.Redo,
				command => command.RootComponent.Descendants,
				notes => T3Logger.Log("Notice", $"Edit_DeleteNoteCount|{notes.Count()}", T3LogType.Info),
				command => command.RootComponent.Model is ITrack),
			// Select cloned tracks or notes
			new CommandFetchRegistrar<CloneComponentCommand, ChartComponent>(commandManager, CommandProcess.Add,
				command => command.ClonedComponents,
				components =>
				{
					selectDataset.Clear();
					var list = components.ToList();
					if (list.Any(component => component.Model is ITrack))
					{
						foreach (var track in list.Where(component => component.Model is ITrack))
						{
							selectDataset.Add(track);
						}
					}
					else
					{
						foreach (var note in list.Where(component => component.Model is INote))
						{
							selectDataset.Add(note);
						}
					}
				}),
			// Select created hold
			new CommandFetchRegistrar<CreateHoldBetweenCommand, ChartComponent?>(commandManager, CommandProcess.Redo,
				command => command.NewHold,
				holds =>
				{
					foreach (var hold in holds.Where(h => h is not null)) selectDataset.Add(hold!);
				})
		};

		// Private
		private ChartSelectDataset selectDataset = default!;
		private CommandManager commandManager = default!;

		// Constructor
		[Inject]
		private void Construct(
			ChartSelectDataset selectDataset,
			CommandManager commandManager)
		{
			this.selectDataset = selectDataset;
			this.commandManager = commandManager;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}