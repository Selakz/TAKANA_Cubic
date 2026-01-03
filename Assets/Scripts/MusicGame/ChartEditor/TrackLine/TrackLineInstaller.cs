#nullable enable

using T3Framework.Preset.Select;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	// TODO: Maybe this and chart select can extract a big common base system?
	public class TrackLineInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<EdgeNodeInitSystem>();

			// SelectInputSystem
			builder.RegisterInstance(new NotifiableProperty<ISelectRaycastMode<EdgeNodeComponent>>
				(PollingRaycastMode<EdgeNodeComponent>.InstanceSole)).AsSelf();
			builder.RegisterInstance(new NotifiableProperty<bool>(false))
				.AsSelf().Keyed("create-node-state");
			builder.RegisterInstance(new NotifiableProperty<int>(ISingleton<TrackLineSetting>.Instance.DefaultEaseId))
				.AsSelf().Keyed("ease-id");
		}
	}
}