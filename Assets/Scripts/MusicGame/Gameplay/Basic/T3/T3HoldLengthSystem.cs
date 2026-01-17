#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3HoldLengthSystem : IInitializable
	{
		private readonly int lengthPriority;
		private readonly IGameAudioPlayer music;
		private readonly IViewPool<ChartComponent> viewPool;
		private readonly SubDataset<ChartComponent, Type> notePool;

		public T3HoldLengthSystem(
			int lengthPriority,
			IGameAudioPlayer music,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.lengthPriority = lengthPriority;
			this.music = music;
			this.viewPool = viewPool;
			notePool = new SubDataset<ChartComponent, Type>(viewPool, new TypeClassifier<IChartModel>(), typeof(Hold));
			notePool.OnDataAdded += OnDataAdded;
			notePool.BeforeDataRemoved += BeforeDataRemoved;
		}

		private void OnDataAdded(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			if (note.Model is Hold hold)
			{
				foreach (var modifier in presenter.HeightModifiers)
				{
					modifier.Register(value => new(value.x,
							hold.TailMovement.GetPos(music.ChartTime) - hold.Movement.GetPos(music.ChartTime)),
						lengthPriority, true);
				}
			}
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			foreach (var modifier in presenter.HeightModifiers)
			{
				modifier.Unregister(lengthPriority, true);
			}
		}

		public void Initialize()
		{
		}
	}
}