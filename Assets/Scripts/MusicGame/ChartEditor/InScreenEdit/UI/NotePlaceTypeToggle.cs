#nullable enable

using T3Framework.Preset.UICollection;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.InScreenEdit.UI
{
	public class NotePlaceTypeToggle : EnumPropertyToggles<SingleNotePlaceType>, ISelfInstaller
	{
		// Serializable and Public
		protected override NotifiableProperty<SingleNotePlaceType> EnumProperty => notePlaceType;

		// Private
		private NotifiableProperty<SingleNotePlaceType> notePlaceType = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<SingleNotePlaceType> notePlaceType)
		{
			this.notePlaceType = notePlaceType;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}