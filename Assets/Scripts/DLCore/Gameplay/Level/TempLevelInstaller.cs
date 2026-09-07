#nullable enable

using DLCore.Utility.Takana;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DLCore.Gameplay.Level
{
	public class TempLevelInstaller : HierarchyInstaller
	{
		[SerializeField] private Camera levelCamara = default!;
		[SerializeField] private GameObject musicPlayerObject = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(levelCamara).As<Camera>().Keyed("stage");
			builder.RegisterInstance(new NotifiableProperty<LevelInfo?>(null)
				{
					Clamp = info =>
					{
						if (info is not null) info.Chart = TakanaToDLConverter.Convert(info.Chart);
						return info;
					}
				})
				.As<NotifiableProperty<LevelInfo?>>();
			builder.RegisterComponent(musicPlayerObject.GetComponent<IGameAudioPlayer>())
				.As<IGameAudioPlayer>()
				.AsSelf();
		}
	}
}