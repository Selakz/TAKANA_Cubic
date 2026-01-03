#nullable enable

using MusicGame.Gameplay.Audio;
using T3Framework.Runtime.Input;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Level
{
	public class LevelInstaller : HierarchyInstaller
	{
		[SerializeField] private InputActionAsset defaultInputAsset = default!;
		[SerializeField] private Camera levelCamara = default!;
		[SerializeField] private GameAudioPlayer musicPlayer = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(levelCamara).As<Camera>().Keyed("stage");
			builder.RegisterInstance(new NotifiableProperty<LevelInfo?>(null)).As<NotifiableProperty<LevelInfo?>>();
			builder.RegisterComponent(musicPlayer).AsSelf();
			ISingleton<InputManager>.Instance.ActionAsset = defaultInputAsset;
		}
	}
}