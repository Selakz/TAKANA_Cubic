#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using T3Framework.Preset.Select;
using T3Framework.Preset.Select.UI;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine.UI
{
	public class TrackNodeRaycastModeInputUI : RaycastModeInputUI<TrackNodeComponent>, ISelfInstaller
	{
		protected override NotifiableProperty<ISelectRaycastMode<TrackNodeComponent>> RaycastMode => raycastMode;

		// Private
		[Inject] private readonly NotifiableProperty<ISelectRaycastMode<TrackNodeComponent>> raycastMode = default!;

		// Defined Functions
		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}
