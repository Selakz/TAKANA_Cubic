#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using VContainer;

namespace MusicGame.Gameplay.Stage
{
	public interface IStageViewGenerateService
	{
		public void StartGenerate(ChartInfo chart, IGameAudioPlayer music);

		public void StopGenerate();

		// TODO: replace with Subject<> when it's implemented later on
		public NotifiableProperty<GameplayStageSkinConfig> OnStageReset { get; }
	}

	public class StageViewGenerateService : HierarchySystem<StageViewGenerateService>, IStageViewGenerateService
	{
		// Serializable and Public
		public override bool AsImplementedInterfaces => true;

		public NotifiableProperty<GameplayStageSkinConfig> OnStageReset => onStageReset ??= new(stageSkinConfig);

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<GameplayStageSkinConfig>(stageSkinConfig, config =>
			{
				viewPool.Prefabs = config.prefabs.Value;
				if (!(chart is null || music is null || viewGenerator is null)) StartGenerate(chart, music);
				OnStageReset.Value = config;
			})
		};

		// Private
		[Inject, Key("stage")] private StageViewPool viewPool = default!;
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
			viewGenerator?.Update(component);

			if (viewPool[component] is not { } handler) return;
			if (newParent is null) handler.transform.SetParent(viewPool.DefaultTransform, false);
			else
			{
				if (viewPool[newParent] is not { } parentHandler)
					return; // Do nothing, and later in Update this view will be released hopefully
				handler.transform.SetParent(parentHandler.transform, false);
			}
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