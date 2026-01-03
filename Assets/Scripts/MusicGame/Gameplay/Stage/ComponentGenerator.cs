#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;

namespace MusicGame.Gameplay.Stage
{
	public class ComponentGenerator : IDisposable
	{
		private readonly ComponentTimeContainer instantiateContainer;
		private readonly ComponentTimeContainer destroyContainer;
		private readonly HashSet<ChartComponent> checkBuffer = new();
		private readonly HashSet<ChartComponent> destroyBuffer = new();
		private T3Time instantiatePointer = T3Time.MinValue;
		private T3Time destroyPointer = T3Time.MinValue;
		private T3Time lastTime = T3Time.MinValue;

		public ComponentGenerator(ChartInfo chart, IModelTimeCalculator calculator)
		{
			instantiateContainer = new ComponentTimeContainer
				(chart, calculator.GetTimeInstantiate, (selfTime, parentTime) => Math.Max(selfTime, parentTime));
			destroyContainer = new ComponentTimeContainer
				(chart, calculator.GetTimeDestroy, (selfTime, parentTime) => Math.Min(selfTime, parentTime));
			chart.OnComponentAdded += OnComponentAdded;
			chart.OnComponentRemoved += OnComponentRemoved;
			chart.OnComponentModelUpdated += OnComponentModelUpdated;
			chart.BeforeComponentParentChanged += BeforeComponentParentChanged;
		}

		public void Update(T3Time time,
			out IEnumerable<ChartComponent> toInstantiate, out IEnumerable<ChartComponent> toDestroy)
		{
			toInstantiate = checkBuffer.Where(c =>
					instantiateContainer.TimeMap.TryGetValue(c, out var instantiateTime) && instantiateTime <= time)
				.ToList();
			toDestroy = checkBuffer.Where(c =>
					(destroyContainer.TimeMap.TryGetValue(c, out var destroyTime) && destroyTime <= time) ||
					(instantiateContainer.TimeMap.TryGetValue(c, out var instantiateTime) && instantiateTime > time))
				.Concat(destroyBuffer).ToList();
			checkBuffer.Clear();
			destroyBuffer.Clear();
			if (time == lastTime) return;

			var instantiateIndex = GetIndex(ref instantiatePointer, instantiateContainer);
			var destroyIndex = GetIndex(ref destroyPointer, destroyContainer);
			if (time > lastTime)
			{
				var instantiateSequence = instantiateContainer.TimeSequence;
				while (instantiateIndex + 1 < instantiateSequence.Count &&
				       instantiateSequence[instantiateIndex + 1] <= time)
				{
					instantiateIndex++;
					toInstantiate = toInstantiate.Concat(
						instantiateContainer.ComponentMap[instantiateSequence[instantiateIndex]]);
				}

				instantiatePointer = instantiateIndex == -1
					? T3Time.MinValue
					: instantiateSequence[instantiateIndex];

				var destroySequence = destroyContainer.TimeSequence;
				while (destroyIndex + 1 < destroySequence.Count &&
				       destroySequence[destroyIndex + 1] <= time)
				{
					destroyIndex++;
					toDestroy = toDestroy.Concat(
						destroyContainer.ComponentMap[destroySequence[destroyIndex]]);
				}

				destroyPointer = destroyIndex == -1
					? T3Time.MinValue
					: destroySequence[destroyIndex];
			}
			else
			{
				var instantiateSequence = instantiateContainer.TimeSequence;
				while (instantiateIndex >= 0 &&
				       instantiateSequence[instantiateIndex] > time)
				{
					toDestroy = toDestroy.Concat(
						instantiateContainer.ComponentMap[instantiateSequence[instantiateIndex]]);
					instantiateIndex--;
				}

				instantiatePointer = instantiateIndex == -1
					? T3Time.MinValue
					: instantiateSequence[instantiateIndex];

				var destroySequence = destroyContainer.TimeSequence;
				while (destroyIndex >= 0 &&
				       destroySequence[destroyIndex] > time)
				{
					toInstantiate = toInstantiate.Concat(
						destroyContainer.ComponentMap[destroySequence[destroyIndex]]);
					destroyIndex--;
				}

				destroyPointer = destroyIndex == -1
					? T3Time.MinValue
					: destroySequence[destroyIndex];
			}

			lastTime = time;
		}

		private static int GetIndex(ref T3Time pointer, ComponentTimeContainer container)
		{
			var sequence = container.TimeSequence;
			if (sequence.Count == 0 || pointer < sequence[0]) return -1;
			var index = sequence.IndexOf(pointer);
			if (index == -1)
			{
				while (index + 1 < sequence.Count && pointer >= sequence[index + 1])
				{
					index++;
				}

				pointer = sequence[index];
			}

			return index;
		}

		private void OnComponentAdded(ChartComponent component) => checkBuffer.Add(component);

		private void OnComponentRemoved(ChartComponent component) => destroyBuffer.Add(component);

		private void OnComponentModelUpdated(ChartComponent component)
		{
			checkBuffer.Add(component);
			checkBuffer.UnionWith(component.Descendants);
		}

		private void BeforeComponentParentChanged(ChartComponent component, ChartComponent? _)
		{
			checkBuffer.Add(component);
			checkBuffer.UnionWith(component.Descendants);
		}

		public void Dispose()
		{
			instantiateContainer.Dispose();
			destroyContainer.Dispose();
		}
	}
}