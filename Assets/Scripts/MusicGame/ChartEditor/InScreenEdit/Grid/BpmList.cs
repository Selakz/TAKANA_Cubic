#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models;
using Newtonsoft.Json.Linq;
using T3Framework.Runtime;
using T3Framework.Runtime.Extensions;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Grid
{
	[ChartTypeMark("bpm")]
	public class BpmList : IChartSerializable, IDictionary<T3Time, float>
	{
		private SortedList<T3Time, float> bpmList;

		public BpmList()
		{
			bpmList = new() { [0] = 100f };
		}

		public BpmList(IDictionary<T3Time, float> bpmList)
		{
			this.bpmList = new(bpmList);
			if (bpmList.Count == 0) bpmList.Add(0, 100f);
		}

		public event Action? OnBpmListUpdate;

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

		public JObject GetSerializationToken()
		{
			var dict = new JObject();
			var list = new JArray();
			foreach (var pair in bpmList)
			{
				list.Add($"({pair.Key}, {pair.Value: 0.0000})");
			}

			dict["list"] = list;
			return dict;
		}

		public static BpmList Deserialize(JObject dict)
		{
			BpmList list = new() { bpmList = new() };
			if (dict["list"] is not JArray array) return list;
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
			var value = Mathf.Max(item.Value, 1f);
			bpmList[item.Key] = value;
			OnBpmListUpdate?.Invoke();
		}

		public void Clear()
		{
			bpmList.Clear();
			bpmList.Add(0, 100f);
			OnBpmListUpdate?.Invoke();
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
			if (ret) OnBpmListUpdate?.Invoke();
			return ret;
		}

		public int Count => bpmList.Count;

		public bool IsReadOnly => false;

		public void Add(T3Time key, float value)
		{
			value = Mathf.Max(value, 1f);
			bpmList[key] = value;
			OnBpmListUpdate?.Invoke();
		}

		public void Reconstruct(IDictionary<T3Time, float> bpmList)
		{
			this.bpmList = new(bpmList);
			OnBpmListUpdate?.Invoke();
		}

		public bool ContainsKey(T3Time key)
		{
			return bpmList.ContainsKey(key);
		}

		public bool Remove(T3Time key)
		{
			var ret = bpmList.Remove(key);
			if (bpmList.Count == 0) bpmList.Add(0, 100f);
			if (ret) OnBpmListUpdate?.Invoke();
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
				OnBpmListUpdate?.Invoke();
			}
		}

		public ICollection<T3Time> Keys => bpmList.Keys;

		public ICollection<float> Values => bpmList.Values;
	}

	public static class BpmListExtensions
	{
		public static BpmList GetsBpmList(this LevelInfo levelInfo)
		{
			if (levelInfo.Chart.HasBpmList()) return levelInfo.Chart.GetsBpmList();
			else if (levelInfo.Preference is EditorPreference preference)
			{
				return preference.BpmList.Count > 0
					? levelInfo.Chart.SetBpmList(preference.BpmList)
					: levelInfo.Chart.GetsBpmList();
			}
			else return levelInfo.Chart.GetsBpmList();
		}

		public static bool HasBpmList(this ChartInfo chart) => chart.EditorConfig.Get<BpmList>("bpm") is { Count: > 0 };

		public static BpmList GetsBpmList(this ChartInfo chart)
		{
			var bpmList = chart.EditorConfig.Get<BpmList>("bpm");
			if (bpmList is null)
			{
				bpmList = new BpmList { [0] = 100 };
				chart.EditorConfig["bpm"] = bpmList;
			}

			return bpmList;
		}

		public static BpmList SetBpmList(this ChartInfo chart, IDictionary<T3Time, float> bpmList)
		{
			var list = chart.GetsBpmList();
			list.Reconstruct(bpmList);
			return list;
		}
	}
}