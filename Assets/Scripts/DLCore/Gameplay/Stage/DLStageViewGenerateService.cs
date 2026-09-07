#nullable enable

using DLCore.Models;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Stage;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace DLCore.Gameplay.Stage
{
	// TODO: Merge to StageViewGenerateService<T>
	public class DLStageViewGenerateService : HierarchySystem<DLStageViewGenerateService>, IStageViewGenerateService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		public NotifiableProperty<GameplayStageSkinConfig> OnStageReset => onStageReset ??= new(stageSkinConfig);

		// Private
		[Inject, Key("stage")] private StageViewPool<DLFlag> viewPool = default!;
		[Inject] private NotifiableProperty<GameplayStageSkinConfig> stageSkinConfig = default!;

		private NotifiableProperty<GameplayStageSkinConfig>? onStageReset;
		private ChartInfo? chart;
		private IGameAudioPlayer? music;
		private TimeWindowViewGenerator<ChartComponent>? viewGenerator;

		// Defined Functions
		public void StartGenerate(ChartInfo chart, IGameAudioPlayer music)
		{
			StopGenerate();
			this.chart = chart;
			this.music = music;
			chart.OnComponentAdded += OnComponentAdded;
			chart.OnComponentRemoved += OnComponentRemoved;
			chart.OnComponentModelUpdated += OnComponentModelUpdated;
			chart.BeforeComponentParentChanged += BeforeParentChanged;
			chart.AfterComponentParentChanged += AfterParentChanged;
			viewGenerator = new(stageSkinConfig.Value.GetViewTimeCalculator());
			foreach (var component in chart) OnComponentAdded(component);
		}

		public void StopGenerate()
		{
			if (chart is not null)
			{
				chart.OnComponentAdded -= OnComponentAdded;
				chart.OnComponentRemoved -= OnComponentRemoved;
				chart.OnComponentModelUpdated -= OnComponentModelUpdated;
				chart.BeforeComponentParentChanged -= BeforeParentChanged;
				chart.AfterComponentParentChanged -= AfterParentChanged;
			}

			viewPool.Clear();
			chart = null;
			music = null;
			viewGenerator?.Clear();
			viewGenerator = null;
		}

		private void OnComponentAdded(ChartComponent component) => viewGenerator?.Add(component);

		private void OnComponentRemoved(ChartComponent component) => viewGenerator?.Remove(component);

		private void OnComponentModelUpdated(ChartComponent component) => viewGenerator?.Update(component);

		private void BeforeParentChanged(ChartComponent component, ChartComponent? newParent)
		{
			if (viewPool[component] is not { } handler) return;
			if (newParent is null) handler.transform.SetParent(viewPool.DefaultTransform, false);
			else
			{
				if (viewPool[newParent] is not { } parentHandler)
					return; // Do nothing, and later in Update this view will be released hopefully
				handler.transform.SetParent(parentHandler.transform, false);
			}
		}

		private void AfterParentChanged(ChartComponent component)
		{
			viewGenerator?.Update(component);
		}

		private void GenerateView(ChartComponent component)
		{
			if (!viewPool.Add(component)) return;

			var handler = viewPool[component]!;
			if (component.Parent is not null)
			{
				if (!viewPool.Contains(component.Parent)) GenerateView(component.Parent);
				handler.transform.SetParent(viewPool[component.Parent]!.transform, false);
			}
		}

		private void ReleaseView(ChartComponent component)
		{
			if (!viewPool.Contains(component)) return;
			foreach (var child in component.Children) ReleaseView(child);
			viewPool.Remove(component);
		}

		// System Functions
		void Update()
		{
			if (music is null || viewGenerator is null) return;
			var time = music.ChartTime;
			viewGenerator.RefreshTime(time, out var toInstantiate, out var toDestroy);
			foreach (var component in toInstantiate) GenerateView(component);
			foreach (var component in toDestroy) ReleaseView(component);
		}
	}
}