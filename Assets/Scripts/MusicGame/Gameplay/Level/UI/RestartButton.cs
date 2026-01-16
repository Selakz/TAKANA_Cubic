#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Level.UI
{
	public class RestartButton : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button button = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(button, () =>
			{
				if (levelInfo.Value is not null) levelInfo.ForceNotify();
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<LevelInfo?> levelInfo)
		{
			this.levelInfo = levelInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}