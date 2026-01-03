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
	public class HoldLengthSpeedSystem : IInitializable
	{
		private readonly int lengthPriority;
		private readonly SpeedDataContainer speedContainer;
		private readonly IViewPool<ChartComponent> viewPool;
		private readonly SubDataset<ChartComponent, Type> notePool;

		public HoldLengthSpeedSystem(
			int lengthPriority,
			SpeedDataContainer speedContainer,
			[Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.lengthPriority = lengthPriority;
			this.speedContainer = speedContainer;
			this.viewPool = viewPool;
			notePool = new SubDataset<ChartComponent, Type>(viewPool, new TypeClassifier<IChartModel>(), typeof(Hold));
		}

		private void OnDataAdded(ChartComponent note)
		{
			var presenter = viewPool[note]!.Script<T3NoteViewPresenter>();
			foreach (var modifier in presenter.HeightModifiers)
			{
				modifier.Register(
					value => new(value.x, value.y * speedContainer.Property.Value.SpeedRate),
					lengthPriority, true);
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
			notePool.OnDataAdded += OnDataAdded;
			notePool.BeforeDataRemoved += BeforeDataRemoved;
		}
	}
}