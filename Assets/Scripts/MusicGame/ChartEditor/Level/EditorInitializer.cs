#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level
{
	public class EditorInitializer : T3MonoBehaviour, ISelfInstaller
	{
		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, () =>
			{
				if (levelInfo.Value is { Preference: EditorPreference preference })
				{
					speed.Value = new T3Speed(preference.Speed);
				}
			})
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private NotifiableProperty<ISpeed> speed = default!;

		// Constructor
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			NotifiableProperty<ISpeed> speed)
		{
			this.levelInfo = levelInfo;
			this.speed = speed;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}