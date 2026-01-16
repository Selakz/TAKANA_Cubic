#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Speed
{
	public class GameplaySpeedLoader : MonoBehaviour, ISelfInstaller
	{
		// Private
		private NotifiableProperty<ISpeed> speed = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<ISpeed> speed)
		{
			this.speed = speed;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// System Functions
		void OnEnable()
		{
			speed.Value.Speed = ISingleton<PlayfieldSetting>.Instance.Speed;
		}
	}
}