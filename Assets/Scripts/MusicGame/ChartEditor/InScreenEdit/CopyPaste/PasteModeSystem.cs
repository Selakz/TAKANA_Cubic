#nullable enable

using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.CopyPaste
{
	public class PasteModeSystem : HierarchySystem<PasteModeSystem>
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("InScreenEdit", "Paste",
				() => mode.Value = PasteMode.NormalPaste),
			new InputRegistrar("InScreenEdit", "ExactPaste",
				() => mode.Value = PasteMode.ExactPaste)
		};

		// Private
		private NotifiableProperty<PasteMode> mode = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<PasteMode> mode)
		{
			this.mode = mode;
		}
	}
}