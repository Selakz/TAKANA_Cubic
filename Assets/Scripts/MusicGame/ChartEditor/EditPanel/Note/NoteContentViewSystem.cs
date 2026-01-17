#nullable enable

using MusicGame.ChartEditor.InScreenEdit;
using MusicGame.ChartEditor.Select;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.EditPanel.Note
{
	public class NoteContentViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolLifetimeRegistrar<ChartComponent>(decoratorPool, handler => new NoteContentRegistrar(
				handler.Script<EditNoteContent>(), decoratorPool[handler]!, selectDataset, music, system))
		};

		// Private
		private IViewPool<ChartComponent> decoratorPool = default!;
		private ChartSelectDataset selectDataset = default!;
		private IGameAudioPlayer music = default!;
		private ChartEditSystem system = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("note-decoration")] IViewPool<ChartComponent> decoratorPool,
			ChartSelectDataset selectDataset,
			IGameAudioPlayer music,
			ChartEditSystem system)
		{
			this.decoratorPool = decoratorPool;
			this.selectDataset = selectDataset;
			this.music = music;
			this.system = system;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}