#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime;
using T3Framework.Static.Collections.Generic;

namespace MusicGame.Gameplay.Stage
{
	public delegate T3Time ModelTimeCalculation(IChartModel? model);

	/// <returns> Self time after restriction. </returns>
	public delegate T3Time ComponentTimeRestriction(T3Time selfTime, T3Time parentTime);

	public class ComponentTimeContainer : IDisposable
	{
		private readonly ChartInfo chart;
		private readonly ModelTimeCalculation calculator;
		private readonly ComponentTimeRestriction restrictor;
		private readonly Dictionary<IChartModel, T3Time> modelTimeMap = new();
		private readonly Dictionary<ChartComponent, T3Time> timeMap = new();

		private readonly HashSetMap<T3Time, ChartComponent> componentMap =
			new(Comparer<T3Time>.Create((t1, t2) => t1.CompareTo(t2)));

		public IReadOnlyDictionary<ChartComponent, T3Time> TimeMap => timeMap;

		public IReadOnlyDictionary<T3Time, HashSet<ChartComponent>> ComponentMap => componentMap.Value;

		public IList<T3Time> TimeSequence => componentMap.Keys;

		public ComponentTimeContainer
			(ChartInfo chart, ModelTimeCalculation calculator, ComponentTimeRestriction restrictor)
		{
			this.chart = chart;
			this.calculator = calculator;
			this.restrictor = restrictor;
			foreach (var component in chart) InitializeTime(component);
			chart.OnComponentAdded += OnComponentAdded;
			chart.OnComponentRemoved += OnComponentRemoved;
			chart.OnComponentModelUpdated += OnComponentModelUpdated;
			chart.BeforeComponentParentChanged += BeforeComponentParentChanged;
		}

		private void OnComponentAdded(ChartComponent component)
		{
			InitializeTime(component);
		}

		private void OnComponentRemoved(ChartComponent component)
		{
			foreach (var child in component.Children) OnComponentRemoved(child);
			if (!timeMap.Remove(component, out var time)) return;
			modelTimeMap.Remove(component.Model);
			componentMap.Remove(time, component);
		}

		private void OnComponentModelUpdated(ChartComponent component)
		{
			modelTimeMap[component.Model] = calculator.Invoke(component.Model);
			AdjustTime(component, component.Parent);
		}

		private void BeforeComponentParentChanged(ChartComponent component, ChartComponent? newParent)
		{
			AdjustTime(component, newParent);
		}

		// Model is not changed, but its parent's time changed
		private void AdjustTime(ChartComponent component, ChartComponent? parent)
		{
			var parentTime = InitializeTime(parent);
			var modelTime = modelTimeMap[component.Model];
			var previousTime = timeMap[component];
			var actualTime = restrictor.Invoke(modelTime, parentTime);
			timeMap[component] = actualTime;
			componentMap.Remove(previousTime, component);
			componentMap.Add(actualTime, component);
			foreach (var child in component.Children) AdjustTime(child, child.Parent);
		}

		public void Dispose()
		{
			chart.OnComponentAdded -= OnComponentAdded;
			chart.OnComponentRemoved -= OnComponentRemoved;
			chart.OnComponentModelUpdated -= OnComponentModelUpdated;
			chart.BeforeComponentParentChanged -= BeforeComponentParentChanged;
		}

		private T3Time InitializeTime(ChartComponent? component)
		{
			if (component is null) return calculator.Invoke(null);
			if (timeMap.TryGetValue(component, out var initializedTime)) return initializedTime;

			T3Time parentTime = component.Parent is not null && timeMap.TryGetValue(component.Parent, out var time)
				? time
				: InitializeTime(component.Parent);
			var modelTime = modelTimeMap.TryGetValue(component.Model, out var time2)
				? time2
				: calculator.Invoke(component.Model);
			var actualTime = restrictor.Invoke(modelTime, parentTime);
			modelTimeMap[component.Model] = modelTime;
			timeMap[component] = actualTime;
			componentMap.Add(actualTime, component);
			return actualTime;
		}
	}
}