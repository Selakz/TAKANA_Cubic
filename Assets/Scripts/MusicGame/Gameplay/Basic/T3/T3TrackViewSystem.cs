#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Track;
using T3Framework.Runtime.ECS;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3TrackViewSystem : IInitializable
	{
		private readonly int positionPriority;
		private readonly int widthPriority;
		private readonly GameAudioPlayer music;
		private readonly SubViewPool<ChartComponent, Type> trackPool;

		public IViewPool<ChartComponent> TrackPool => trackPool;

		public T3TrackViewSystem(
			int positionPriority,
			int widthPriority,
			GameAudioPlayer music,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.positionPriority = positionPriority;
			this.widthPriority = widthPriority;
			this.music = music;
			trackPool = new SubViewPool<ChartComponent, Type>(viewPool, new TypeClassifier<IChartModel>(),
				typeof(Track));
			trackPool.OnDataAdded += OnDataAdded;
			trackPool.BeforeDataRemoved += BeforeDataRemoved;
		}

		private void OnDataAdded(ChartComponent track)
		{
			var presenter = trackPool[track]!.Script<T3TrackViewPresenter>();
			presenter.PositionModifier.Register(
				_ => (track.Model as ITrack)!.Movement.GetPos(music.ChartTime),
				positionPriority, true);
			presenter.WidthModifier.Register(
				_ => (track.Model as ITrack)!.Movement.GetWidth(music.ChartTime),
				widthPriority, true);
		}

		private void BeforeDataRemoved(ChartComponent track)
		{
			var presenter = trackPool[track]!.Script<T3TrackViewPresenter>();
			presenter.PositionModifier.Unregister(positionPriority, true);
			presenter.WidthModifier.Unregister(widthPriority, true);
		}

		public void Initialize()
		{
		}
	}
}