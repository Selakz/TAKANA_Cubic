#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelResult
{
	public class ResultLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Private
		private NotifiableProperty<ResultInfo?> resultInfo = default!;

		// Static
		private static ResultInfo? toLoadResultInfo;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<ResultInfo?> resultInfo)
		{
			this.resultInfo = resultInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public static void SetResultInfo(ResultInfo levelInfo) => toLoadResultInfo = levelInfo;

		// System Functions
		void Start()
		{
			if (toLoadResultInfo is null) return;
			resultInfo.Value = toLoadResultInfo;
			toLoadResultInfo = null;
		}
	}
}