#nullable enable

using T3Framework.Preset.Select;
using T3Framework.Preset.Select.UI;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class DirectNodeRaycastModeInputUI : RaycastModeInputUI<DirectNodeComponent>, ISelfInstaller
	{
		protected override NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>> RaycastMode => raycastMode;

		// Private
		private NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>> raycastMode = default!;

		// Defined Functions
		[Inject]
		protected void Construct(NotifiableProperty<ISelectRaycastMode<DirectNodeComponent>> raycastMode)
		{
			this.raycastMode = raycastMode;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}