#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime;
using T3Framework.Static.Collections.Generic;

namespace MusicGame.Gameplay.Stage
{
	public interface ITimeCalculator<T> where T : class
	{
		public T3Time GetTimeInstantiate(T? item);

		public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime);

		public T3Time GetTimeDestroy(T? item);

		public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime);

		public T? GetParent(T item);

		public IEnumerable<T> GetChildren(T item);
	}

	internal class TimeContainer<T> where T : class
	{
		private readonly ITimeCalculator<T> timeCalculator;
		private readonly Dictionary<T, T3Time> timeMap = new();
		private readonly HashSetMap<T3Time, T> componentMap = new(Comparer<T3Time>.Create((x, y) => x.CompareTo(y)));
		private readonly Func<T?, T3Time> timeGetter;
		private readonly Func<T3Time, T3Time, T3Time> timeRestriction;

		public IReadOnlyDictionary<T, T3Time> TimeMap => timeMap;

		public IReadOnlyDictionary<T3Time, HashSet<T>> ComponentMap => componentMap.Value;

		public IList<T3Time> TimeSequence => componentMap.Keys;

		public TimeContainer(ITimeCalculator<T> timeCalculator, bool isInstantiate)
		{
			this.timeCalculator = timeCalculator;
			timeGetter = isInstantiate
				? timeCalculator.GetTimeInstantiate
				: timeCalculator.GetTimeDestroy;
			timeRestriction = isInstantiate
				? timeCalculator.InstantiateTimeRestriction
				: timeCalculator.DestroyTimeRestriction;
		}

		public void Add(T item)
		{
			InitializeTime(item);
		}

		public void Remove(T item)
		{
			foreach (var child in timeCalculator.GetChildren(item)) Remove(child);
			if (!timeMap.Remove(item, out var time)) return;
			componentMap.Remove(time, item);
		}

		public void Update(T item)
		{
			var parent = timeCalculator.GetParent(item);
			AdjustTime(item, parent);
		}

		public IEnumerable<T> GetItemsInRange(T3Time start, T3Time end)
		{
			var keys = componentMap.Keys;
			if (keys.Count == 0) return Enumerable.Empty<T>();
			int left = BinarySearchLeft(keys, start);
			int right = BinarySearchRight(keys, end);
			if (left > right) return Enumerable.Empty<T>();
			var result = new HashSet<T>();
			for (int i = left; i <= right; i++)
			{
				foreach (var item in componentMap[keys[i]]) result.Add(item);
			}

			return result;
		}

		private static int BinarySearchLeft(IList<T3Time> keys, T3Time target)
		{
			int left = 0, right = keys.Count - 1;
			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				if (keys[mid] < target) left = mid + 1;
				else right = mid - 1;
			}

			return left;
		}

		private static int BinarySearchRight(IList<T3Time> keys, T3Time target)
		{
			int left = 0, right = keys.Count - 1;
			while (left <= right)
			{
				int mid = left + (right - left) / 2;
				if (keys[mid] > target) right = mid - 1;
				else left = mid + 1;
			}

			return right;
		}

		public void Clear()
		{
			timeMap.Clear();
			componentMap.Clear();
		}

		private void AdjustTime(T item, T? parent)
		{
			var parentTime = InitializeTime(parent);
			var selfTime = timeGetter.Invoke(item);
			var previousTime = timeMap[item];
			var actualTime = timeRestriction.Invoke(selfTime, parentTime);
			timeMap[item] = actualTime;
			componentMap.Remove(previousTime, item);
			componentMap.Add(actualTime, item);
			foreach (var child in timeCalculator.GetChildren(item)) AdjustTime(child, item);
		}

		private T3Time InitializeTime(T? item)
		{
			if (item is null) return timeGetter.Invoke(null);
			if (timeMap.TryGetValue(item, out var initializedTime)) return initializedTime;

			var parent = timeCalculator.GetParent(item);
			T3Time parentTime = parent is not null && timeMap.TryGetValue(parent, out var time)
				? time
				: InitializeTime(parent);
			var selfTime = timeGetter.Invoke(item);
			var actualTime = timeRestriction.Invoke(selfTime, parentTime);
			timeMap[item] = actualTime;
			componentMap.Add(actualTime, item);
			return actualTime;
		}
	}

	/// <summary>
	/// Decides which views to instantiate and destroy in the current time window.
	/// </summary>
	public class TimeWindowViewGenerator<T> where T : class
	{
		private readonly TimeContainer<T> instantiateContainer;
		private readonly TimeContainer<T> destroyContainer;
		private readonly HashSet<T> pendingCheck = new();
		private readonly HashSet<T> pendingRemove = new();
		private readonly HashSet<T> activeItems = new();
		private T3Time lastTime = T3Time.MinValue;

		public TimeWindowViewGenerator(ITimeCalculator<T> timeCalculator)
		{
			instantiateContainer = new TimeContainer<T>(timeCalculator, true);
			destroyContainer = new TimeContainer<T>(timeCalculator, false);
		}

		public void Add(T item)
		{
			instantiateContainer.Add(item);
			destroyContainer.Add(item);
			pendingCheck.Add(item);
			pendingRemove.Remove(item);
		}

		public void Remove(T item)
		{
			instantiateContainer.Remove(item);
			destroyContainer.Remove(item);
			pendingCheck.Remove(item);
			pendingRemove.Add(item);
		}

		public void Update(T item)
		{
			instantiateContainer.Update(item);
			destroyContainer.Update(item);
			pendingCheck.Add(item);
		}

		public void RefreshTime(T3Time time, out IEnumerable<T> toInstantiate, out IEnumerable<T> toDestroy)
		{
			var toInstantiateSet = new HashSet<T>();
			var toDestroySet = new HashSet<T>();

			foreach (var item in pendingRemove)
			{
				if (activeItems.Contains(item))
				{
					toDestroySet.Add(item);
					activeItems.Remove(item);
				}
			}

			pendingRemove.Clear();

			foreach (var item in pendingCheck)
			{
				bool shouldInstantiate = instantiateContainer.TimeMap.TryGetValue(item, out var instantiateTime) &&
				                         instantiateTime <= time;
				bool shouldDestroy = destroyContainer.TimeMap.TryGetValue(item, out var destroyTime) &&
				                     destroyTime <= time;
				if (activeItems.Contains(item))
				{
					if (!shouldInstantiate || shouldDestroy)
					{
						toDestroySet.Add(item);
						activeItems.Remove(item);
					}
				}
				else if (shouldInstantiate && !shouldDestroy)
				{
					toInstantiateSet.Add(item);
					activeItems.Add(item);
				}
			}

			pendingCheck.Clear();

			if (time != lastTime)
			{
				var (start, end) = time > lastTime ? (lastTime, time) : (time, lastTime);
				var instantiateItems = instantiateContainer.GetItemsInRange(start, end);
				var destroyItems = destroyContainer.GetItemsInRange(start, end);

				if (time > lastTime)
				{
					foreach (var item in instantiateItems)
					{
						if (!activeItems.Contains(item)) toInstantiateSet.Add(item);
						activeItems.Add(item);
					}

					foreach (var item in destroyItems)
					{
						if (toInstantiateSet.Contains(item)) toInstantiateSet.Remove(item);
						else if (activeItems.Contains(item)) toDestroySet.Add(item);
						activeItems.Remove(item);
					}
				}
				else
				{
					foreach (var item in destroyItems)
					{
						if (!activeItems.Contains(item)) toInstantiateSet.Add(item);
						activeItems.Add(item);
					}

					foreach (var item in instantiateItems)
					{
						if (toInstantiateSet.Contains(item)) toInstantiateSet.Remove(item);
						else if (activeItems.Contains(item)) toDestroySet.Add(item);
						activeItems.Remove(item);
					}
				}
			}

			lastTime = time;
			toInstantiate = toInstantiateSet;
			toDestroy = toDestroySet;
		}

		public void Clear()
		{
			instantiateContainer.Clear();
			destroyContainer.Clear();
			pendingCheck.Clear();
			pendingRemove.Clear();
			activeItems.Clear();
			lastTime = T3Time.MinValue;
		}
	}
}