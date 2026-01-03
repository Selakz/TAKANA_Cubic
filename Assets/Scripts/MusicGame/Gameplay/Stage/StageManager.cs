#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Stage
{
	public class StageManager : ITickable
	{
		private readonly IViewPool<ChartComponent> viewPool;

		private ChartInfo? chart;
		private GameAudioPlayer? music;
		private ComponentGenerator? generator;

		public StageManager([Key("stage")] IViewPool<ChartComponent> viewPool)
		{
			this.viewPool = viewPool;
		}

		public void StartGenerate(ChartInfo chart, GameAudioPlayer music, IModelTimeCalculator calculator)
		{
			StopGenerate();
			this.chart = chart;
			this.music = music;
			chart.BeforeComponentParentChanged += BeforeParentChanged;
			generator = new(chart, calculator);
		}

		public void StopGenerate()
		{
			if (chart is not null) chart.BeforeComponentParentChanged += BeforeParentChanged;
			chart = null;
			music = null;
			viewPool.Clear();
			generator?.Dispose();
			generator = null;
		}

		public void Tick()
		{
			if (music is null) return;
			var time = music.ChartTime;
			generator!.Update(time, out var toInstantiate, out var toDestroy);
			foreach (var component in toInstantiate) GenerateView(component);
			foreach (var component in toDestroy) ReleaseView(component);
		}

		private void BeforeParentChanged(ChartComponent component, ChartComponent? newParent)
		{
			var handler = viewPool[component];
			if (handler is null) return;
			if (newParent is null) handler.transform.SetParent(viewPool.DefaultTransform, false);
			else
			{
				var parentHandler = viewPool[newParent];
				if (parentHandler is not null)
				{
					handler.transform.SetParent(parentHandler.transform, false);
				}
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
	}
}