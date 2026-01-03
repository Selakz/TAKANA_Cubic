#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Models.Note;
using MusicGame.Models.Note.Combo;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class NoteSortedDataset : IReadOnlyDataset<ChartComponent>
	{
		private readonly ChartInfo chart;
		private readonly Dictionary<ChartComponent, IComboInfo> dataset = new();
		private readonly List<T3Time> times = new();

		public int Count => dataset.Count;

		public IReadOnlyList<T3Time> Times => times;

		public IComboInfo? this[ChartComponent component] => dataset.GetValueOrDefault(component);

		public event Action<ChartComponent>? OnDataAdded;
		public event Action<ChartComponent>? OnDataAddedInherit;
		public event Action<ChartComponent>? BeforeDataRemoved;
		public event Action<ChartComponent>? BeforeDataRemovedInherit;
		public event Action<ChartComponent>? OnDataUpdated;

		public NoteSortedDataset(ChartInfo chart)
		{
			this.chart = chart;
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
			if (component.Model is not INote note) return;
			var combo = T3ComboGetter.GetComboInfo(note, null);

			dataset[component] = combo;
			foreach (var time in combo.ComboTimes)
			{
				int insertIndex = GetLowerBoundIndex(time);
				times.Insert(insertIndex, time);
			}

			OnDataAddedInherit?.Invoke(component);
			OnDataAdded?.Invoke(component);
		}

		private void OnComponentRemoved(ChartComponent component)
		{
			if (!dataset.Remove(component, out var combo)) return;

			foreach (var time in combo.ComboTimes)
			{
				int index = GetLowerBoundIndex(time);
				times.RemoveAt(index);
			}

			BeforeDataRemoved?.Invoke(component);
			BeforeDataRemovedInherit?.Invoke(component);
		}

		private void OnComponentModelUpdated(ChartComponent component)
		{
			if (!dataset.ContainsKey(component) || component.Model is not INote note) return;

			var oldCombo = dataset[component];
			foreach (var time in oldCombo.ComboTimes)
			{
				int index = GetLowerBoundIndex(time);
				times.RemoveAt(index);
			}

			var newCombo = T3ComboGetter.GetComboInfo(note, oldCombo);
			dataset[component] = newCombo;
			foreach (var time in newCombo.ComboTimes)
			{
				int insertIndex = GetLowerBoundIndex(time);
				times.Insert(insertIndex, time);
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