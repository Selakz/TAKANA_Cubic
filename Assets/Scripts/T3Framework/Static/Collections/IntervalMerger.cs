#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace T3Framework.Static.Collections
{
	public struct Interval
	{
		public float Left { get; set; }

		public float Right { get; set; }

		public Interval(float left, float right)
		{
			Left = left;
			Right = right;
		}
	}

	public static class IntervalMerger
	{
		private static readonly IComparer<Interval> comparer =
			Comparer<Interval>.Create((a, b) => a.Left.CompareTo(b.Left));

		/// <summary> Merged intervals are set in the given list in place. </summary>
		/// <returns> The count of merged intervals. </returns>
		public static int Merge(List<Interval> intervals, int count = -1)
		{
			if (count < 0) count = intervals.Count;
			if (count == 0) return count;
			intervals.Sort(0, count, comparer);
			int mergedCount = 0;
			for (int i = 1; i < count; i++)
			{
				var target = intervals[mergedCount];
				var current = intervals[i];
				if (current.Left <= target.Right)
				{
					intervals[mergedCount] = new(target.Left, Mathf.Max(current.Right, target.Right));
				}
				else
				{
					mergedCount++;
					intervals[mergedCount] = current;
				}
			}

			return mergedCount + 1;
		}
	}
}