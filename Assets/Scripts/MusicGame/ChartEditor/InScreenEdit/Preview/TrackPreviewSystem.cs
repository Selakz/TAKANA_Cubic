#nullable enable

using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Speed;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.InScreenEdit.Preview
{
	public class TrackPreviewSystem : HierarchySystem<TrackPreviewSystem>
	{
		// Serializable and Public
		[SerializeField] private ViewPoolInstaller installer;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new AutoViewPoolRegistrar<TrackRawInfo>(dataset, viewPool, true),
			new ViewPoolRegistrar<TrackRawInfo>(viewPool,
				ViewPoolRegistrar<TrackRawInfo>.RegisterTarget.Get,
				handler =>
				{
					var info = viewPool[handler]!;
					var decorator = handler.Script<TrackDecorator>();
					UpdateDecorator(info, decorator);
				}),
			new DatasetRegistrar<TrackRawInfo>(dataset,
				DatasetRegistrar<TrackRawInfo>.RegisterTarget.DataUpdated,
				info =>
				{
					var decorator = viewPool[info]!.Script<TrackDecorator>();
					UpdateDecorator(info, decorator);
				})
		};

		// Private
		private IDataset<TrackRawInfo> dataset = default!;
		private IViewPool<TrackRawInfo> viewPool = default!;
		private IGameAudioPlayer music = default!;
		private NotifiableProperty<ISpeed> speed = default!;

		// Constructor
		[Inject]
		private void Construct(
			IDataset<TrackRawInfo> dataset,
			IViewPool<TrackRawInfo> viewPool,
			IGameAudioPlayer music,
			NotifiableProperty<ISpeed> speed)
		{
			this.dataset = dataset;
			this.viewPool = viewPool;
			this.music = music;
			this.speed = speed;
		}

		public override void SelfInstall(IContainerBuilder builder)
		{
			base.SelfInstall(builder);
			builder.RegisterViewPool<TrackRawInfo>(installer);
		}

		// Defined Functions
		private void UpdateDecorator(TrackRawInfo info, TrackDecorator decorator)
		{
			if (info.Track.Model is not ITrack model) return;
			var x = model.Movement.GetPos(model.TimeMin);
			var y = model.TimeMin.Second - music.ChartTime.Second;
			y *= speed.Value.SpeedRate;
			decorator.transform.localPosition = decorator.transform.localPosition with { x = x, y = y };
			decorator.Indicator.size = decorator.Indicator.size with { x = model.Movement.GetWidth(model.TimeMin) };
		}
	}
}