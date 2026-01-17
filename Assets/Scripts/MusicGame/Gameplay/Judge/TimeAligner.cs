#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Judge
{
	// TODO: Delete this
	public class TimeAligner : T3MonoBehaviour, ISelfInstaller
	{
		// Private
		private JudgeTimeAudioPlayer music = default!;

		// Constructor
		[Inject]
		private void Construct(JudgeTimeAudioPlayer music)
		{
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public T3Time GetChartTime(double inputTime) => music.GetChartTime(inputTime);
	}
}