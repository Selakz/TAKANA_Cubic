#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Movement;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace MusicGame.Models.Note.Movement
{
	/// <summary> V1LS: 1 Dimension and linear and by speed </summary>
	[StringTypeMark("v1ls")]
	public struct V1LSMoveItem : IEquatable<V1LSMoveItem>, IStringSerializable
	{
		public float Speed { get; set; }

		public V1LSMoveItem(float speed)
		{
			Speed = speed;
		}

		public bool Equals(V1LSMoveItem other) => Speed.Equals(other.Speed);

		public override bool Equals(object? obj) => obj is V1LSMoveItem other && Equals(other);

		public override int GetHashCode() => HashCode.Combine(Speed);

		public string Serialize()
		{
			return $"({Speed:0.00})";
		}

		public static V1LSMoveItem Deserialize(string s)
		{
			var match = RegexHelper.MatchTuple(s, 1);
			var speed = float.Parse(match.Groups[1].Value);
			return new V1LSMoveItem(speed);
		}
	}

	[ChartTypeMark("v1ls")]
	public class V1LSMoveList : IMovement<float>, IMoveList<V1LSMoveItem>, IChartSerializable
	{
		private readonly SortedList<T3Time, V1LSMoveItem> list;
		private readonly SortedList<T3Time, float> anchors;
		private int lastIndex = 0;
		private float baseTimeRelativePos = 0;
		private T3Time baseTime;
		private float basePosition;

		public T3Time BaseTime
		{
			get => baseTime;
			set
			{
				baseTime = value;
				baseTimeRelativePos = GetRelativePos(baseTime);
			}
		}

		public float BasePosition
		{
			get => basePosition;
			set
			{
				basePosition = value;
				baseTimeRelativePos = GetRelativePos(baseTime);
			}
		}

		/// <summary> Guaranteed to be greater than 1 </summary>
		public int Count => list.Count;

		public KeyValuePair<T3Time, V1LSMoveItem> this[int index] => new(list.Keys[index], list.Values[index]);

		public V1LSMoveList(T3Time time, float position, float speed = 1f)
		{
			baseTime = time;
			basePosition = position;
			list = new() { { time, new V1LSMoveItem(speed) } };
			anchors = new() { { time, position } };
			baseTimeRelativePos = GetRelativePos(BaseTime);
		}

		public V1LSMoveList(IDictionary<T3Time, V1LSMoveItem> items, T3Time baseTime, float basePosition)
		{
			this.baseTime = baseTime;
			this.basePosition = basePosition;
			list = new();
			anchors = new();
			foreach (var pair in items) Insert(pair.Key, pair.Value);
			if (Count == 0)
			{
				list.Add(baseTime, new V1LSMoveItem(1f));
				anchors.Add(baseTime, basePosition);
			}
			else
			{
				ConstructAnchors();
			}

			baseTimeRelativePos = GetRelativePos(BaseTime);
		}

		public float GetPos(T3Time time)
		{
			var relativePos = GetRelativePos(time);
			return relativePos + (BasePosition - baseTimeRelativePos);
		}

		public void Nudge(T3Time distance)
		{
			var pairs = list.ToArray();
			list.Clear();
			foreach (var (time, item) in pairs) list.Add(time + distance, item);
			ConstructAnchors();
			BaseTime += distance;
		}

		public void Shift(float offset) => BaseTime += offset;

		private float GetRelativePos(T3Time time)
		{
			if (time < list.Keys[0])
				return anchors.Values[0] + list.Values[0].Speed * (time - list.Keys[0]).Second;
			if (time >= list.Keys[^1])
				return anchors.Values[^1] + list.Values[^1].Speed * (time - list.Keys[^1]).Second;

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
				return Mathf.Lerp(anchors.Values[lastIndex], anchors.Values[lastIndex + 1], t);
			}
			else
			{
				var search = list.BinarySearch(time);
				if (search >= 0) return anchors[time];

				var startIndex = ~search - 1;
				lastIndex = Mathf.Max(0, startIndex);
				indexTime = list.Keys[lastIndex];
				T3Time nextIndexTime = list.Keys[lastIndex + 1];
				float t = (float)(time - indexTime) / (nextIndexTime - indexTime);
				return Mathf.Lerp(anchors.Values[lastIndex], anchors.Values[lastIndex + 1], t);
			}
		}

		public bool Insert(T3Time time, V1LSMoveItem item)
		{
			list[time] = item;
			ConstructAnchors();
			baseTimeRelativePos = GetRelativePos(BaseTime);
			return true;
		}

		public bool Remove(T3Time time)
		{
			if (Count == 1) return false;
			if (list.Remove(time))
			{
				anchors.Remove(time);
				ConstructAnchors();
				baseTimeRelativePos = GetRelativePos(BaseTime);
				return true;
			}

			return false;
		}

		public bool Contains(V1LSMoveItem item) => list.ContainsValue(item);

		public bool TryGet(T3Time time, out V1LSMoveItem item)
		{
			return list.TryGetValue(time, out item);
		}

		// TODO: Buffer for anchors also
		private void ConstructAnchors()
		{
			anchors.Clear();
			float currentPosition = 0;
			anchors[list.Keys[0]] = currentPosition;
			for (int i = 1; i < list.Count; i++)
			{
				currentPosition += list.Values[i - 1].Speed * (list.Keys[i] - list.Keys[i - 1]).Second;
				anchors[list.Keys[i]] = currentPosition;
			}
		}

		public IEnumerator<KeyValuePair<T3Time, V1LSMoveItem>> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public JObject GetSerializationToken()
		{
			var array = new JObject();
			foreach (var pair in list)
			{
				array[pair.Key.ToString()] = pair.Value.Serialize();
			}

			var token = new JObject
			{
				["baseTime"] = BaseTime.Milli,
				["basePosition"] = BasePosition,
				["list"] = array
			};
			return token;
		}

		public static V1LSMoveList Deserialize(JObject dict)
		{
			T3Time baseTime = dict["baseTime"]!.Value<int>();
			float basePosition = dict["basePosition"]!.Value<float>();
			if (dict["list"] is not JObject array) return new(baseTime, basePosition);
			Dictionary<T3Time, V1LSMoveItem> list = new();
			foreach (var pair in array)
			{
				list.Add(T3Time.Parse(pair.Key), V1LSMoveItem.Deserialize(pair.Value!.Value<string>()!));
			}

			return new V1LSMoveList(list, baseTime, basePosition);
		}
	}
}