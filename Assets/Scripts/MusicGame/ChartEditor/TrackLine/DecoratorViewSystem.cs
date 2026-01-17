#nullable enable

using MusicGame.ChartEditor.Decoration.Track;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLine
{
	public class DecoratorViewSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Private
		private IViewPool<ChartComponent> decoratorPool = default!;
		private IGameAudioPlayer music = default!;
		private NotifiableProperty<ISpeed> speed = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ViewPoolRegistrar<ChartComponent>(decoratorPool,
				ViewPoolRegistrar<ChartComponent>.RegisterTarget.Get,
				handler => UpdateView(decoratorPool[handler]!)),
			new DatasetRegistrar<ChartComponent>(decoratorPool,
				DatasetRegistrar<ChartComponent>.RegisterTarget.DataUpdated,
				UpdateView)
		};

		// Defined Functions
		[Inject]
		private void Construct(
			[Key("track-decoration")] IViewPool<ChartComponent> decoratorPool,
			IGameAudioPlayer music,
			NotifiableProperty<ISpeed> speed)
		{
			this.decoratorPool = decoratorPool;
			this.music = music;
			this.speed = speed;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		private void UpdateView(ChartComponent component)
		{
			if (component.Model is not ITrack track || decoratorPool[component] is not { } handler) return;
			var decorator = handler.Script<TrackDecorator>();
			decorator.Indicator.size = decorator.Indicator.size with
			{
				x = track.Movement.GetWidth(track.TimeStart)
			};
			var position = decorator.Indicator.transform.localPosition;
			position.x = track.Movement.GetPos(track.TimeStart);
			decorator.Indicator.transform.localPosition = position;
		}

		// System Functions
		void Update()
		{
			foreach (var component in decoratorPool)
			{
				if (component.Model is not ITrack track) continue;
				var handler = decoratorPool[component]!;
				var position = handler.transform.localPosition;
				position.y = speed.Value.SpeedRate * (track.TimeStart - music.ChartTime).Second;
				handler.transform.localPosition = position;
			}
		}
	}
}