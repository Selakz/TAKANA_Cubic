#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Select;
using T3Framework.Preset.Select.UI;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Select.UI
{
	public class ChartRaycastModeInputUI : RaycastModeInputUI<ChartComponent>, ISelfInstaller
	{
		protected override NotifiableProperty<ISelectRaycastMode<ChartComponent>> RaycastMode => raycastMode;

		// Private
		private NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode = default!;

		// Defined Functions
		[Inject]
		protected void Construct(NotifiableProperty<ISelectRaycastMode<ChartComponent>> raycastMode)
		{
			this.raycastMode = raycastMode;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}