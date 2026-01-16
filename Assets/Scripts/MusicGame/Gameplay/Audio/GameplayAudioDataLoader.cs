#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Audio
{
	public class GameplayAudioDataLoader : MonoBehaviour, ISelfInstaller
	{
		// Private
		private GameAudioPlayer music = default!;

		// Constructor
		[Inject]
		private void Construct(GameAudioPlayer music)
		{
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// System Functions
		void OnEnable()
		{
			music.AudioDeviation = ISingleton<PlayfieldSetting>.Instance.AudioDeviation.Value;
		}
	}
}