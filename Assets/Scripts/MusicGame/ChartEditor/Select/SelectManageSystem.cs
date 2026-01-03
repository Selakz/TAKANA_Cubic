#nullable enable

using System;
using System.Linq;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Preset.Event;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace MusicGame.ChartEditor.Select
{
	public class SelectManageSystem : T3System
	{
		private readonly NotifiableProperty<LevelInfo?> levelInfo;
		private readonly ChartSelectDataset dataset;

		// Serializable and Public
		protected override IEventRegistrar[] ActiveRegistrars => new IEventRegistrar[]
		{
			// Clear dataset when chart change
			new PropertyRegistrar<LevelInfo?>(levelInfo, () => dataset.Clear()),
			// Clear other type of components when current selecting change
			new PropertyRegistrar<ChartComponent?>(dataset.CurrentSelecting, () =>
			{
				var selecting = dataset.CurrentSelecting.Value;
				if (selecting is null) return;
				switch (selecting.Model)
				{
					case ITrack:
						if (dataset.CurrentSelecting.LastValue?.Model is ITrack) return;
						RemoveNotOfType<ITrack>();
						break;
					case INote:
						if (dataset.CurrentSelecting.LastValue?.Model is INote) return;
						RemoveNotOfType<INote>();
						break;
				}

				return;

				void RemoveNotOfType<T>()
				{
					var selected = dataset.ToArray();
					foreach (var component in selected)
					{
						if (component.Model is not T) dataset.Remove(component);
					}
				}
			}),
			new PropertyNestedRegistrar<LevelInfo?>(levelInfo, info => CustomRegistrar.Generic<Action<ChartComponent>>(
				action => info!.Chart.OnComponentRemoved += action,
				action => info!.Chart.OnComponentRemoved -= action,
				component => dataset.Remove(component)))
		};

		// Defined Functions
		public SelectManageSystem(
			NotifiableProperty<LevelInfo?> levelInfo,
			ChartSelectDataset dataset) : base(true)
		{
			this.levelInfo = levelInfo;
			this.dataset = dataset;
		}
	}
}