using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace MusicGame.Components.Movement
{
	/// <summary> V1LS: 1 Dimension and linear and by speed </summary>
	public struct V1LSMoveItem : IMoveItem, IEquatable<V1LSMoveItem>
	{
		public T3Time Time { get; set; }
		public float Speed { get; set; }

		public V1LSMoveItem(T3Time time, float speed)
		{
			Time = time;
			Speed = speed;
		}

		public bool Equals(V1LSMoveItem other)
		{
			return Time.Equals(other.Time) && Speed.Equals(other.Speed);
		}

		public override bool Equals(object obj)
		{
			return obj is V1LSMoveItem other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Time, Speed);
		}

		public override string ToString()
		{
			return $"({Time}, {Speed:0.00})";
		}

		public static V1LSMoveItem Parse(string s)
		{
			var match = RegexHelper.MatchTuple(s, 2);
			var time = int.Parse(match.Groups[1].Value);
			var speed = float.Parse(match.Groups[2].Value);
			return new V1LSMoveItem(time, speed);
		}

		public IMoveItem SetTime(T3Time time)
		{
			return new V1LSMoveItem(time, Speed);
		}
	}

	public class V1LSMoveList : IMovement<float>, IMoveList<V1LSMoveItem>
	{
		public static string TypeMark => "v1ls";

		private readonly SortedList<T3Time, V1LSMoveItem> list;
		private readonly SortedList<T3Time, float> anchors;
		private int lastIndex = 0;
		private float baseTimeRelativePos = 0;
		private T3Time baseTime;
		private float basePosition;

		public event Action OnMovementUpdated = delegate { };

		public T3Time BaseTime
		{
			get => baseTime;
			set
			{
				baseTime = value;
				baseTimeRelativePos = GetRelativePos(baseTime);
				OnMovementUpdated.Invoke();
			}
		}

		public float BasePosition
		{
			get => basePosition;
			set
			{
				basePosition = value;
				baseTimeRelativePos = GetRelativePos(baseTime);
				OnMovementUpdated.Invoke();
			}
		}

		/// <summary> Guaranteed to be greater than 1 </summary>
		public int Count => list.Count;

		public V1LSMoveItem this[int index] => list.Values[index];

		public V1LSMoveList(T3Time time, float position, float speed = 1f)
		{
			baseTime = time;
			basePosition = position;
			list = new() { { time, new V1LSMoveItem(time, speed) } };
			anchors = new() { { time, position } };
			baseTimeRelativePos = GetRelativePos(BaseTime);
		}

		public V1LSMoveList(IEnumerable<V1LSMoveItem> items, T3Time baseTime, float basePosition)
		{
			this.baseTime = baseTime;
			this.basePosition = basePosition;
			list = new();
			anchors = new();
			foreach (var item in items) Insert(item);
			if (Count == 0)
			{
				list.Add(baseTime, new V1LSMoveItem(baseTime, 1f));
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

		private float GetRelativePos(T3Time time)
		{
			if (time < list.Keys[0])
				return anchors.Values[0] + list.Values[0].Speed * (time - list.Keys[0]);
			if (time >= list.Keys[^1])
				return anchors.Values[^1] + list.Values[^1].Speed * (time - list.Keys[^1]);

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

		public bool Insert(V1LSMoveItem item)
		{
			list[item.Time] = item;
			ConstructAnchors();
			baseTimeRelativePos = GetRelativePos(BaseTime);
			OnMovementUpdated.Invoke();
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
				OnMovementUpdated.Invoke();
				return true;
			}

			return false;
		}

		public bool Contains(V1LSMoveItem item)
		{
			return list.Keys.Contains(item.Time) && list[item.Time].Equals(item);
		}

		public bool TryGet(T3Time time, out V1LSMoveItem item)
		{
			return list.TryGetValue(time, out item);
		}

		public IMovement<float> Clone(T3Time timeOffset, float positionOffset)
		{
			return new V1LSMoveList(list.Values.Select(item => new V1LSMoveItem(item.Time + timeOffset, item.Speed)),
				BaseTime + timeOffset, BasePosition + positionOffset);
		}

		// TODO: Buffer for anchors also
		private void ConstructAnchors()
		{
			anchors.Clear();
			float currentPosition = 0;
			anchors[list.Keys[0]] = currentPosition;
			for (int i = 1; i < list.Count; i++)
			{
				currentPosition += list.Values[i - 1].Speed * (list.Keys[i] - list.Keys[i - 1]);
				anchors[list.Keys[i]] = currentPosition;
			}
		}

		public IEnumerator<V1LSMoveItem> GetEnumerator()
		{
			return list.Select(a => a.Value).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public JToken GetSerializationToken()
		{
			var array = new JArray();
			foreach (var moveItem in list.Values)
			{
				array.Add(moveItem.ToString());
			}

			var token = new JObject
			{
				["baseTime"] = BaseTime.Milli,
				["basePosition"] = BasePosition,
				["list"] = array
			};
			return token;
		}

		public static V1LSMoveList Deserialize(JToken token)
		{
			if (token is not JContainer container) return default;
			T3Time baseTime = container["baseTime"]!.Value<int>();
			float basePosition = container["basePosition"]!.Value<float>();
			List<V1LSMoveItem> list = container["list"]!.Select(
				item => V1LSMoveItem.Parse(item.Value<string>())).ToList();

			return new V1LSMoveList(list, baseTime, basePosition);
		}
	}
}