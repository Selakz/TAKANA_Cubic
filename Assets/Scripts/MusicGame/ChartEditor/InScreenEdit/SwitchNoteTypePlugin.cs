#nullable enable

using MusicGame.Models;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class SwitchNoteTypePlugin : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "SwitchCreateType", SwitchNoteType)
		};

		// Private
		private NotifiableProperty<T3Flag> noteType = default!;

		// Defined Functions
		[Inject]
		private void Construct(
			NotifiableProperty<T3Flag> noteType)
		{
			this.noteType = noteType;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void SwitchNoteType()
		{
			var newNoteType = noteType.Value switch
			{
				T3Flag.Tap => T3Flag.Slide,
				T3Flag.Slide => T3Flag.Hold,
				T3Flag.Hold => T3Flag.Tap,
				_ => T3Flag.Tap
			};
			noteType.Value = newNoteType;
		}
	}
}