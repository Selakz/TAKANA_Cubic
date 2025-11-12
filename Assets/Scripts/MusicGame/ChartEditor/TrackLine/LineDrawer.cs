using System.Collections.Generic;
using T3Framework.Runtime.Extensions;
using T3Framework.Static.Easing;
using UnityEngine;

namespace MusicGame.ChartEditor.TrackLine
{
	public static class LineDrawer
	{
		public static void DrawCurve(LineRenderer line, Vector2 start, Vector2 end, string label, float precision)
		{
			const float z = -0.01f;
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

		private static void PointsToMesh(Mesh mesh, Vector2[] points, float lineWidth)
		{
			if (points.Length <= 1) return;

			var width = lineWidth / 2;
			Vector3[] vertices = new Vector3[points.Length * 2];
			Vector2[] uv = new Vector2[points.Length * 2];
			Vector3[] normals = new Vector3[points.Length * 2];
			int[] triangles = new int[(points.Length - 1) * 6];

			// the first two vertices
			Vector2 directionFirst = (points[1] - points[0]).normalized;
			Vector2 normalFirst = new(-directionFirst.y, directionFirst.x);
			vertices[0] = points[0] + width * normalFirst;
			uv[0] = new(0, 0);
			normals[0] = normalFirst;
			vertices[1] = points[0] - width * normalFirst;
			uv[1] = new(1, 0);
			normals[1] = -normalFirst;

			// the vertices in the middle
			Vector2 direction2 = directionFirst;
			Vector2 sidePoint2 = vertices[0];
			Vector2 sidePoint4 = vertices[1];
			for (int i = 0; i < points.Length - 2; i++)
			{
				var direction1 = direction2;
				direction2 = (points[i + 2] - points[i + 1]).normalized;
				if (direction2 == Vector2.zero) direction2 = (points[i + 2] - points[i]).normalized;
				var normal2 = new Vector2(-direction2.y, direction2.x);
				var sidePoint1 = sidePoint2;
				sidePoint2 = points[i + 1] + width * normal2;
				var leftPoint =
					MathfExtended.GetIntersectionPoint2D(direction1, sidePoint1, direction2, sidePoint2) ?? sidePoint2;
				vertices[(i + 1) * 2] = leftPoint;
				uv[(i + 1) * 2] = new(0, 0);
				normals[(i + 1) * 2] = (leftPoint - points[i + 1]).normalized;
				var sidePoint3 = sidePoint4;
				sidePoint4 = points[i + 1] - width * normal2;
				var rightPoint =
					MathfExtended.GetIntersectionPoint2D(direction1, sidePoint3, direction2, sidePoint4) ?? sidePoint4;
				vertices[(i + 1) * 2 + 1] = rightPoint;
				uv[(i + 1) * 2 + 1] = new(1, 0);
				normals[(i + 1) * 2 + 1] = (rightPoint - points[i + 1]).normalized;
			}

			// the last two vertices
			var normalLast = new Vector2(-direction2.y, direction2.x);
			vertices[^2] = points[^1] + width * normalLast;
			uv[^2] = new(0, 0);
			normals[^2] = normalLast;
			vertices[^1] = points[^1] - width * normalLast;
			uv[^1] = new(1, 0);
			normals[^1] = -normalLast;

			// triangles
			for (int i = 0; i < points.Length - 1; i++)
			{
				triangles[i * 6] = i * 2;
				triangles[i * 6 + 1] = i * 2 + 2;
				triangles[i * 6 + 2] = i * 2 + 1;

				triangles[i * 6 + 3] = i * 2 + 1;
				triangles[i * 6 + 4] = i * 2 + 2;
				triangles[i * 6 + 5] = i * 2 + 3;
			}

			if (vertices.Length < mesh.vertexCount)
			{
				mesh.triangles = triangles;
				mesh.vertices = vertices;
			}
			else
			{
				mesh.vertices = vertices;
				mesh.triangles = triangles;
			}

			mesh.uv = uv;
			mesh.normals = normals;
		}

		/// <summary> Recommend to draw in editor mode and save to asset. </summary>
		public static void DrawMesh(Mesh mesh, Eases ease, float lineWidth, float width, float height, float precision,
			float maxSegment)
		{
			List<Vector2> points = new() { new(0, 0) };
			switch (ease)
			{
				case Eases.Unmove:
					points.Add(new(0, height));
					break;
				case Eases.Linear:
					break;
				default:
					precision = Mathf.Max(precision, height / maxSegment);
					for (float i = precision; i < height; i += precision)
					{
						points.Add(new(ease.CalcCoord(0, width, i / height), i));
					}

					break;
			}

			points.Add(new(width, height));
			PointsToMesh(mesh, points.ToArray(), lineWidth);
		}
	}
}