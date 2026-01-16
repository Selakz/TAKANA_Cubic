#nullable enable

using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.LevelResult
{
	public class LevelResultInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<NotifiableProperty<ResultInfo?>>(Lifetime.Singleton)
				.WithParameter("initialValue", default(ResultInfo?));
		}
	}
}