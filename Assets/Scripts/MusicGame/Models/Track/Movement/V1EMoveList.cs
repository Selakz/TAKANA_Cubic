using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Easing;
using T3Framework.Static.Movement;
using UnityEngine;

namespace MusicGame.Models.Track.Movement
{
	/// <summary> V1E: 1 Dimension and by easing </summary>
	[StringTypeMark("v1e")]
	public class V1EMoveItem : IPositionMoveItem<float>, IStringSerializable
	{
		public float Position { get; set; }

		public Eases Ease { get; set; }

		public float Add(float x, float y) => x + y;

		public float GetPosition(T3Time thisTime, T3Time targetTime, T3Time nextTime, float nextPosition)
		{
			var t = (targetTime.Second - thisTime.Second) / (nextTime.Second - thisTime.Second);
			return Ease.Opposite().CalcCoord(Position, nextPosition, t);
		}

		public IPositionMoveItem<float> SetPosition(float newPosition) => new V1EMoveItem(newPosition, Ease);

		public V1EMoveItem(float position, Eases ease)
		{
			Position = position;
			Ease = ease;
		}

		public string Serialize() => $"({Position:0.00}, {Ease.GetString()})";

		public static V1EMoveItem Deserialize(string content)
		{
			var match = RegexHelper.MatchTuple(content, 2);
			var position = float.Parse(match.Groups[1].Value);
			var ease = CurveCalculator.GetEaseByName(match.Groups[2].Value.Trim());
			return new V1EMoveItem(position, ease);
		}
	}

	[ChartTypeMark("v1e")]
	public class V1EMoveList : IMovement<float>, IMoveList<V1EMoveItem>, IChartSerializable
	{
		private readonly SortedList<T3Time, V1EMoveItem> list;
		private int lastIndex = 0;

		public event Action OnMovementUpdated = delegate { };

		public int Count => list.Count;

		public V1EMoveItem this[int index] => list.Values[index];

		public V1EMoveList()
		{
			list = new();
		}

		public V1EMoveList(IEnumerable<V1EMoveItem> items)
		{
			list = new();
			foreach (var item in items) Insert(item);
		}

		public float GetPos(T3Time time)
		{
			if (Count == 0) return 0;
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
				float t = (float)(time - indexTime) / (nextIndexTime - indexTime);
				return list[indexTime].Ease.Opposite()
					.CalcCoord(list[indexTime].Position, list[nextIndexTime].Position, t);
			}
			else
			{
				var search = list.BinarySearch(time);
				if (search >= 0) return list[time].Position;

				var startIndex = ~search - 1;
				lastIndex = Mathf.Max(0, startIndex);
				indexTime = list.Keys[lastIndex];
				T3Time nextIndexTime = list.Keys[lastIndex + 1];
				float t = (float)(time - indexTime) / (nextIndexTime - indexTime);
				return list[indexTime].Ease.Opposite().CalcCoord(indexTime, nextIndexTime, t);
			}
		}

		public bool Insert(V1EMoveItem item)
		{
			lastIndex = 0;
			list[0] = item;
			OnMovementUpdated.Invoke();
			return true;
		}

		public bool Insert(T3Time time, V1EMoveItem item)
		{
			throw new NotImplementedException();
		}

		public bool Remove(T3Time time)
		{
			lastIndex = 0;
			if (list.Remove(time))
			{
				OnMovementUpdated.Invoke();
				return true;
			}

			return false;
		}

		public bool Contains(V1EMoveItem item)
		{
			return list.ContainsKey(0) && list[0].Equals(item);
		}

		public bool TryGet(T3Time time, out V1EMoveItem item)
		{
			return list.TryGetValue(time, out item);
		}

		public void Nudge(T3Time distance)
		{
			throw new NotImplementedException();
		}

		public void Shift(float offset)
		{
			throw new NotImplementedException();
		}

		public IMovement<float> Clone(T3Time timeOffset, float positionOffset)
		{
			if (Count == 0) return new V1EMoveList();
			return new V1EMoveList(list.Values.Select(item =>
				new V1EMoveItem(item.Position + positionOffset, item.Ease)));
		}

		public IEnumerator<KeyValuePair<T3Time, V1EMoveItem>> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public JObject GetSerializationToken()
		{
			var jArray = new JArray();
			foreach (var moveItem in list.Values)
			{
				jArray.Add(((IStringSerializable)moveItem).Serialize(true));
			}

			var token = new JObject()
			{
				["list"] = jArray
			};

			return token;
		}

		public static V1EMoveList Deserialize(JObject dict)
		{
			List<V1EMoveItem> list = dict["list"]!
				.Select(item => IStringSerializable.Deserialize<V1EMoveItem>(item.Value<string>())).ToList();
			return new V1EMoveList(list);
		}
	}
}