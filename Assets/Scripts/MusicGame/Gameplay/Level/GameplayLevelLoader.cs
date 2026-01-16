#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Level
{
	public class GameplayLevelLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Static
		private static LevelInfo? toLoadLevelInfo;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelInfo = levelInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public static void SetLevelInfo(LevelInfo levelInfo) => toLoadLevelInfo = levelInfo;

		// System Functions
		void Start()
		{
			if (toLoadLevelInfo is null) return;
			levelInfo.Value = toLoadLevelInfo;
			toLoadLevelInfo = null;
		}
	}
}