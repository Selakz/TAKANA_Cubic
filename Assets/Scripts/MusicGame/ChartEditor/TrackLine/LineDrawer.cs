using System.Collections.Generic;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public static class LineDrawer
	{
		public static void DrawCurve(LineRenderer line, Vector2 start, Vector2 end, string label, float precision)
		{
			const float z = -1f;
			List<Vector3> points = new();
			switch (label)
			{
				case "u":
					points.Add(new(start.x, start.y, z));
					points.Add(new(start.x, end.y, z));
					break;
				case "s":
					points.Add(new(start.x, start.y, z));
					break;
				default:
					var ease = CurveCalculator.GetEaseByName(label);
					for (float i = start.y; i < end.y; i += precision)
					{
						points.Add(new(ease.CalcCoord(start.x, end.x, (i - start.y) / (end.y - start.y)), i, z));
					}

					break;
			}

			points.Add(new(end.x, end.y, z));
			line.positionCount = points.Count;
			line.SetPositions(points.ToArray());
		}

		public static void DrawCurve(EdgeCollider2D line, Vector2 start, Vector2 end, string label, float precision)
		{
			List<Vector2> points = new();
			// 对两种简单曲线做优化
			if (label == "u")
			{
				points.Add(new(start.x, start.y));
				points.Add(new(start.x, end.y));
			}
			else if (label == "s")
			{
				points.Add(new(start.x, start.y));
			}
			else
			{
				var ease = CurveCalculator.GetEaseByName(label);
				for (float i = start.y; i < end.y; i += precision)
				{
					points.Add(new(ease.CalcCoord(start.x, end.x, (i - start.y) / (end.y - start.y)), i));
				}
			}

			points.Add(new(end.x, end.y));
			line.points = points.ToArray();
		}

		/// <summary> 动态确定精度：在最高精度为0.1的前提下，最多将曲线分为100份 </summary>
		public static void DrawCurve(LineRenderer line, Vector2 start, Vector2 end, string label)
		{
			float precision = Mathf.Max(0.1f, (end.y - start.y) / 100f);
			DrawCurve(line, start, end, label, precision);
		}

		/// <summary> 动态确定精度：在最高精度为0.2的前提下，最多将曲线分为100份 </summary>
		public static void DrawCurve(EdgeCollider2D line, Vector2 start, Vector2 end, string label)
		{
			float precision = Mathf.Max(0.1f, (end.y - start.y) / 100f);
			DrawCurve(line, start, end, label, precision);
		}
	}
}