#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class NoteSortedDataset : IReadOnlyDataset<ChartComponent>
	{
		private readonly ChartInfo chart;
		private readonly IComboFactory comboFactory;
		private readonly Dictionary<ChartComponent, List<IComboItem>> dataset = new();
		private readonly List<T3Time> times = new();

		public int Count => dataset.Count;

		public IReadOnlyList<T3Time> Times => times;

		public IReadOnlyList<IComboItem>? this[ChartComponent component] => dataset.GetValueOrDefault(component);

		public event Action<ChartComponent>? OnDataAdded;
		public event Action<ChartComponent>? OnDataAddedInherit;
		public event Action<ChartComponent>? BeforeDataRemoved;
		public event Action<ChartComponent>? BeforeDataRemovedInherit;
		public event Action<ChartComponent>? OnDataUpdated;

		public NoteSortedDataset(ChartInfo chart, IComboFactory comboFactory)
		{
			this.chart = chart;
			this.comboFactory = comboFactory;
			chart.OnComponentAdded += OnComponentAdded;
			chart.OnComponentRemoved += OnComponentRemoved;
			chart.OnComponentModelUpdated += OnComponentModelUpdated;
			foreach (var component in chart) OnComponentAdded(component);
		}

		public bool Contains(ChartComponent component) => dataset.ContainsKey(component);

		public int GetLowerBoundIndex(T3Time time)
		{
			int left = 0;
			int right = times.Count;
			while (left < right)
			{
				int mid = left + (right - left) / 2;
				if (times[mid] < time) left = mid + 1;
				else right = mid;
			}

			return left;
		}

		private void OnComponentAdded(ChartComponent component)
		{
			if (component.Model is not INote) return;
			var combos = new List<IComboItem>(comboFactory.CreateCombo(component));

			dataset[component] = combos;
			foreach (var combo in combos)
			{
				int insertIndex = GetLowerBoundIndex(combo.ExpectedTime);
				times.Insert(insertIndex, combo.ExpectedTime);
			}

			OnDataAddedInherit?.Invoke(component);
			OnDataAdded?.Invoke(component);
		}

		private void OnComponentRemoved(ChartComponent component)
		{
			if (!dataset.Remove(component, out var combos)) return;

			foreach (var combo in combos)
			{
				int index = GetLowerBoundIndex(combo.ExpectedTime);
				times.RemoveAt(index);
			}

			BeforeDataRemoved?.Invoke(component);
			BeforeDataRemovedInherit?.Invoke(component);
		}

		private void OnComponentModelUpdated(ChartComponent component)
		{
			if (!dataset.ContainsKey(component) || component.Model is not INote) return;

			var oldCombos = dataset[component];
			foreach (var combo in oldCombos)
			{
				int index = GetLowerBoundIndex(combo.ExpectedTime);
				times.RemoveAt(index);
			}

			var newCombos = new List<IComboItem>(comboFactory.CreateCombo(component));
			dataset[component] = newCombos;
			foreach (var combo in newCombos)
			{
				int insertIndex = GetLowerBoundIndex(combo.ExpectedTime);
				times.Insert(insertIndex, combo.ExpectedTime);
			}

			OnDataUpdated?.Invoke(component);
		}

		public IEnumerator<ChartComponent> GetEnumerator() => dataset.Keys.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void Dispose()
		{
			chart.OnComponentAdded -= OnComponentAdded;
			chart.OnComponentRemoved -= OnComponentRemoved;
			chart.OnComponentModelUpdated -= OnComponentModelUpdated;
		}
	}
}
