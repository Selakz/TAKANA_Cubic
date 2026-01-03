#nullable enable

using T3Framework.Preset.Select;
using T3Framework.Preset.Select.UI;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class NodeRaycastModeInputUI : RaycastModeInputUI<EdgeNodeComponent>, ISelfInstaller
	{
		protected override NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> RaycastMode => raycastMode;

		// Private
		private NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> raycastMode = default!;

		// Defined Functions
		[Inject]
		protected void Construct(NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>> raycastMode)
		{
			this.raycastMode = raycastMode;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}