#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Audio
{
	public class GameplayAudioDataLoader : T3MonoBehaviour, ISelfInstaller
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, info =>
			{
				music.AudioDeviation =
					ISingleton<PlayfieldSetting>.Instance.AudioDeviation.Value +
					(info?.Preference is GameplayPreference preference ? preference.SongDeviation : 0);
			})
		};

		// Private
		private IGameAudioPlayer music = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Constructor
		[Inject]
		private void Construct(
			IGameAudioPlayer music,
			NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.music = music;
			this.levelInfo = levelInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}