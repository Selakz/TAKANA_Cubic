#nullable enable

using System.Linq;
using MusicGame.ChartEditor.InScreenEdit.Grid;
using T3Framework.Runtime;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace EditorPlugin.Shared.To
{
	public class RawBpmListData
	{
		private readonly BpmList list;

		public int size => list.Count;

		public RawBpmListData(BpmList list)
		{
			this.list = list;
		}

		public int getFloorTime(int timeMilli, int gridDivision)
		{
			return list.GetFloorTime(new T3Time(timeMilli), gridDivision, out _);
		}

		public int getCeilTime(int timeMilli, int gridDivision)
		{
			return list.GetCeilTime(new T3Time(timeMilli), gridDivision, out _);
		}

		public bool has(int key) => list.ContainsKey(key);

		public float? get(int key) => list.TryGetValue(key, out float value) ? value : null;

		public bool delete(int key) => list.Remove(key);

		public void clear() => list.Clear();

		public int[] keys() => list.Keys.Select(key => key.Milli).ToArray();

		public float[] values() => list.Values.ToArray();

		public RawBpmListData set(int key, float value)
		{
			list[key] = Mathf.Max(value, 1f);
			return this;
		}
	}
}
