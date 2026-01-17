using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Audio
{
	public class AudioPlayHandler : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new InputRegistrar("EditorBasic", "Pause", TogglePlay),
			new InputRegistrar("EditorBasic", "ForcePause", ForcePause)
		};

		// Private
		private IGameAudioPlayer music;

		// Defined Functions
		[Inject]
		private void Construct(IGameAudioPlayer music) => this.music = music;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public void TogglePlay(InputAction.CallbackContext context)
		{
			if (music.IsPlaying) music.Pause();
			else music.Play();
		}

		public void ForcePause(InputAction.CallbackContext context)
		{
			if (ISingleton<EditorSetting>.Instance.ShouldForcePause) music.Pause();
		}
	}
}