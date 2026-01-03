#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;

namespace T3Framework.Static.Movement
{
	public interface IPositionMoveItem<TPosition>
	{
		TPosition Position { get; set; }

		public TPosition Add(TPosition x, TPosition y);

		public TPosition GetPosition(T3Time thisTime, T3Time targetTime, T3Time nextTime, TPosition nextPosition);

		public IPositionMoveItem<TPosition> SetPosition(TPosition newPosition);
	}

	public class PositionMoveList<TPosition> : IMoveList<IPositionMoveItem<TPosition>>, IMovement<TPosition>
	{
		private readonly SortedList<T3Time, IPositionMoveItem<TPosition>> list;
		private int lastIndex = 0;

		public int Count => list.Count;

		public KeyValuePair<T3Time, IPositionMoveItem<TPosition>> this[int index]
			=> new(list.Keys[index], list.Values[index]);

		public T3Time? this[IPositionMoveItem<TPosition> item]
			=> list.Keys.FirstOrDefault(time => Equals(list[time], item));

		public PositionMoveList() => list = new();

		public PositionMoveList(IDictionary<T3Time, IPositionMoveItem<TPosition>> items)
		{
			list = new();
			foreach (var pair in items) Insert(pair.Key, pair.Value);
		}

		public TPosition GetPos(T3Time time)
		{
			if (Count == 0) return default!;
			if (time < list.Keys[0]) return list.Values[0].Position;
			if (time >= list.Keys[^1]) return list.Values[^1].Position;
			T3Time indexTime = list.Keys[lastIndex];
			if (time >= indexTime)
			{
				while (lastIndex < list.Count - 1)
				{
					if (list.Keys[lastIndex + 1] > time) break;
					lastIndex++;
				}

				indexTime = list.Keys[lastIndex];
				T3Time nextIndexTime = list.Keys[lastIndex + 1];
				return list[indexTime].GetPosition(indexTime, time, nextIndexTime, list[nextIndexTime].Position);
			}
			else
			{
				var search = list.BinarySearch(time);
				if (search >= 0) return list[time].Position;

				var startIndex = ~search - 1;
				lastIndex = Math.Max(0, startIndex);
				indexTime = list.Keys[lastIndex];
				T3Time nextIndexTime = list.Keys[lastIndex + 1];
				return list[indexTime].GetPosition(indexTime, time, nextIndexTime, list[nextIndexTime].Position);
			}
		}

		public bool Insert(T3Time time, IPositionMoveItem<TPosition> item)
		{
			lastIndex = 0;
			list[time] = item;
			return true;
		}

		public bool Remove(T3Time time)
		{
			lastIndex = 0;
			return list.Remove(time);
		}

		public bool Contains(IPositionMoveItem<TPosition> item) => list.ContainsValue(item);

		public bool TryGet(T3Time time, out IPositionMoveItem<TPosition> item) => list.TryGetValue(time, out item);

		public void Nudge(T3Time distance)
		{
			var pairs = list.ToArray();
			list.Clear();
			foreach (var (time, item) in pairs) list.Add(time + distance, item);
			lastIndex = 0;
		}

		public void Shift(TPosition offset)
		{
			foreach (var item in list.Values) item.Position = item.Add(item.Position, offset);
		}

		public int BinarySearch(T3Time time) => list.BinarySearch(time);

		public IEnumerator<KeyValuePair<T3Time, IPositionMoveItem<TPosition>>> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}