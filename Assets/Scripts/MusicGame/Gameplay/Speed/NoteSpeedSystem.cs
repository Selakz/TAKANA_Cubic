#nullable enable

using System;
using MusicGame.Gameplay.Basic.T3;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using MusicGame.Models.Note;
using T3Framework.Runtime.ECS;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Speed
{
	public class NoteSpeedSystem : IInitializable
	{
		private readonly int positionPriority;
		private readonly SpeedDataContainer speedContainer;
		private readonly IViewPool<ChartComponent> viewPool;
		private readonly SubDataset<ChartComponent, Type> notePool;

		public NoteSpeedSystem(
			int positionPriority,
			SpeedDataContainer speedContainer,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.positionPriority = positionPriority;
			this.speedContainer = speedContainer;
			this.viewPool = viewPool;
			notePool = new SubDataset<ChartComponent, Type>(viewPool, new TypeClassifier<IChartModel>(),
				typeof(Hit), typeof(Hold));
		}

		private void OnDataAdded(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			presenter.PositionModifier.Register(
				value => new(value.x, value.y * speedContainer.Property.Value.SpeedRate),
				positionPriority, true);
		}

		private void BeforeDataRemoved(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			presenter.PositionModifier.Unregister(positionPriority, true);
		}

		public void Initialize()
		{
			notePool.OnDataAdded += OnDataAdded;
			notePool.BeforeDataRemoved += BeforeDataRemoved;
		}
	}
}