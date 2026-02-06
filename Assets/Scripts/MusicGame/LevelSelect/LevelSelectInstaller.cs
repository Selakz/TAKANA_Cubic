#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.LevelSelect
{
	public class LevelSelectInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<IDataset<LevelComponent<GameplayPreference>>,
				HashDataset<LevelComponent<GameplayPreference>>>(Lifetime.Singleton);
			builder.Register<NotifiableProperty<RawLevelInfo<GameplayPreference>?>>(Lifetime.Singleton)
				.WithParameter("initialValue", default(RawLevelInfo<GameplayPreference>?));
			builder.Register<NotifiableProperty<int>>(Lifetime.Singleton) // selected difficulty
				.WithParameter("initialValue", 0);
		}
	}
}