using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.Components.Movement
{
	/// <summary> V1E: 1 Dimension and by easing </summary>
	public struct V1EMoveItem : IMoveItem, IEquatable<V1EMoveItem>
	{
		public T3Time Time { get; set; }

		public float Position { get; set; }

		public Eases Ease { get; set; }

		public V1EMoveItem(T3Time time, float position, Eases ease)
		{
			Time = time;
			Position = position;
			Ease = ease;
		}

		public bool Equals(V1EMoveItem other)
		{
			return Time.Equals(other.Time) && Position.Equals(other.Position) && Ease == other.Ease;
		}

		public override bool Equals(object obj)
		{
			return obj is V1EMoveItem other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Time, Position, (int)Ease);
		}

		public override string ToString()
		{
			return $"({Time}, {Position:0.00}, {Ease.GetString()})";
		}

		public static V1EMoveItem Parse(string s)
		{
			var match = RegexHelper.MatchTuple(s, 3);
			var time = int.Parse(match.Groups[1].Value);
			var position = float.Parse(match.Groups[2].Value);
			var ease = CurveCalculator.GetEaseByName(match.Groups[3].Value.Trim());
			return new V1EMoveItem(time, position, ease);
		}

		public IMoveItem SetTime(T3Time time)
		{
			return new V1EMoveItem(time, Position, Ease);
		}
	}

	public class V1EMoveList : IMovement<float>, IMoveList, IMoveList<V1EMoveItem>
	{
		public static string TypeMark => "v1e";

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
			list[item.Time] = item;
			OnMovementUpdated.Invoke();
			return true;
		}

		bool IMoveList.Insert(IMoveItem item)
		{
			return item is V1EMoveItem i && Insert(i);
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
			return list.ContainsKey(item.Time) && list[item.Time].Equals(item);
		}

		public bool TryGet(T3Time time, out V1EMoveItem item)
		{
			return list.TryGetValue(time, out item);
		}

		bool IMoveList.TryGet(T3Time time, out IMoveItem item)
		{
			var result = list.TryGetValue(time, out var i);
			item = i;
			return result;
		}

		public IMovement<float> Clone(T3Time timeOffset, float positionOffset)
		{
			if (Count == 0) return new V1EMoveList();
			return new V1EMoveList(list.Values.Select(item =>
				new V1EMoveItem(item.Time + timeOffset, item.Position + positionOffset, item.Ease)));
		}

		public IEnumerator<V1EMoveItem> GetEnumerator()
		{
			return list.Select(pair => pair.Value).GetEnumerator();
		}

		IEnumerator<IMoveItem> IEnumerable<IMoveItem>.GetEnumerator()
		{
			return list.Select(pair => pair.Value).Cast<IMoveItem>().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public JToken GetSerializationToken()
		{
			var jArray = new JArray();
			foreach (var moveItem in list.Values)
			{
				jArray.Add(moveItem.ToString());
			}

			var token = new JObject()
			{
				["list"] = jArray
			};

			return token;
		}

		public static V1EMoveList Deserialize(JToken token)
		{
			if (token is not JContainer container) return default;
			List<V1EMoveItem> list = container["list"]!.Select(item => V1EMoveItem.Parse(item.Value<string>()))
				.ToList();

			return new V1EMoveList(list);
		}
	}
}