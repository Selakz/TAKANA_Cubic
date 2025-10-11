using System;
using System.Collections;
using System.Collections.Generic;
using MusicGame.Components;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	public class BpmList : ISerializable, IDictionary<T3Time, float>
	{
		public static string TypeMark => "bpmList";

		private SortedList<T3Time, float> bpmList;

		public BpmList()
		{
			bpmList = new();
		}

		public BpmList(IDictionary<T3Time, float> bpmList)
		{
			this.bpmList = new(bpmList);
		}

		public event Action OnBpmListUpdate = delegate { };

		/// <summary> The result will never be equal to given time, unless the time is the start time of this list. </summary>
		public T3Time GetFloorTime(T3Time time, int gridDivision, out int gridIndexSinceLastBpm)
		{
			if (bpmList.Count == 0 || bpmList.Keys[0] > time)
			{
				gridIndexSinceLastBpm = 0;
				return 0;
			}

			// index is at the last bpm where its time <= given time
			int index = 0;
			while (index + 1 < bpmList.Count && bpmList.Keys[index + 1] <= time) index++;
			float timePerGrid = 60f / bpmList.Values[index] / gridDivision;
			gridIndexSinceLastBpm = Mathf.FloorToInt((time - bpmList.Keys[index]) / timePerGrid);
			var result = bpmList.Keys[index] + gridIndexSinceLastBpm * timePerGrid;
			while (result == time)
			{
				if (gridIndexSinceLastBpm > 0)
				{
					gridIndexSinceLastBpm--;
				}
				else
				{
					if (index == 0) return time;
					index--;
					timePerGrid = 60f / bpmList.Values[index] / gridDivision;
					gridIndexSinceLastBpm = Mathf.FloorToInt((time - bpmList.Keys[index]) / timePerGrid);
				}

				result = bpmList.Keys[index] + gridIndexSinceLastBpm * timePerGrid;
			}

			return result;
		}

		/// <summary> The result will never be equal to given time. </summary>
		public T3Time GetCeilTime(T3Time time, int gridDivision, out int gridIndexSinceLastBpm)
		{
			if (bpmList.Count == 0)
			{
				gridIndexSinceLastBpm = 0;
				return T3Time.MaxValue;
			}

			// index is at the last bpm where its time <= given time
			int index = 0;
			while (index + 1 < bpmList.Count && bpmList.Keys[index + 1] <= time) index++;
			float timePerGrid = 60f / bpmList.Values[index] / gridDivision;
			gridIndexSinceLastBpm = Mathf.CeilToInt((time - bpmList.Keys[index]) / timePerGrid);
			var result = bpmList.Keys[index] + gridIndexSinceLastBpm * timePerGrid;
			if (result == time)
			{
				gridIndexSinceLastBpm++;
				result = bpmList.Keys[index] + gridIndexSinceLastBpm * timePerGrid;
			}

			if (index + 1 < bpmList.Count && bpmList.Keys[index + 1] <= result)
			{
				gridIndexSinceLastBpm = 0;
				result = bpmList.Keys[index + 1];
			}

			return result;
		}

		public Dictionary<T3Time, float> ToDictionary()
		{
			return new(bpmList);
		}

		public JToken GetSerializationToken()
		{
			var token = new JArray();
			foreach (var pair in bpmList)
			{
				token.Add($"({pair.Key}, {pair.Value})");
			}

			return token;
		}

		public static BpmList Deserialize(JToken token)
		{
			if (token is not JArray array) return default;
			BpmList list = new() { bpmList = new() };
			foreach (var bpmItem in array)
			{
				var match = RegexHelper.MatchTuple(bpmItem.ToString(), 2);
				T3Time time = int.Parse(match.Groups[1].Value);
				float bpm = float.Parse(match.Groups[2].Value);
				list.bpmList[time] = bpm;
			}

			return list;
		}

		public IEnumerator<KeyValuePair<T3Time, float>> GetEnumerator()
		{
			return bpmList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(KeyValuePair<T3Time, float> item)
		{
			OnBpmListUpdate.Invoke();
			var value = Mathf.Max(item.Value, 1f);
			bpmList[item.Key] = value;
		}

		public void Clear()
		{
			if (bpmList.Count > 0) OnBpmListUpdate.Invoke();
			bpmList.Clear();
		}

		public bool Contains(KeyValuePair<T3Time, float> item)
		{
			return bpmList.ContainsKey(item.Key) && Mathf.Approximately(bpmList[item.Key], item.Value);
		}

		public void CopyTo(KeyValuePair<T3Time, float>[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		public bool Remove(KeyValuePair<T3Time, float> item)
		{
			var ret = bpmList.Remove(item.Key);
			if (ret) OnBpmListUpdate.Invoke();
			return ret;
		}

		public int Count => bpmList.Count;

		public bool IsReadOnly => false;

		public void Add(T3Time key, float value)
		{
			value = Mathf.Max(value, 1f);
			bpmList[key] = value;
			OnBpmListUpdate.Invoke();
		}

		public bool ContainsKey(T3Time key)
		{
			return bpmList.ContainsKey(key);
		}

		public bool Remove(T3Time key)
		{
			var ret = bpmList.Remove(key);
			if (ret) OnBpmListUpdate.Invoke();
			return ret;
		}

		public bool TryGetValue(T3Time key, out float value)
		{
			return bpmList.TryGetValue(key, out value);
		}

		public float this[T3Time key]
		{
			get => bpmList[key];
			set
			{
				bpmList[key] = value;
				OnBpmListUpdate.Invoke();
			}
		}

		public ICollection<T3Time> Keys => bpmList.Keys;

		public ICollection<float> Values => bpmList.Values;
	}
}