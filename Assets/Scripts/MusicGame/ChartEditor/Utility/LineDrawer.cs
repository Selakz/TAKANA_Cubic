using System.Collections.Generic;
using UnityEngine;

// 简易画线
public static class LineDrawer
{
    public static void DrawCurve(LineRenderer line, Vector2 start, Vector2 end, string label, float width, float precision)
    {
        line.startWidth = line.endWidth = width;
        float z = -1f;
        List<Vector3> points = new();
        // 对两种简单曲线做优化
        if (label == "u")
        {
            points.Add(new(start.x, start.y, z));
            points.Add(new(start.x, end.y, z));
        }
        else if (label == "s")
        {
            points.Add(new(start.x, start.y, z));
        }
        else
        {
            for (float i = start.y; i < end.y; i += precision)
            {
                points.Add(new(CurveCalculator.CalcCurveCoord(start.x, end.x, (i - start.y) / (end.y - start.y), label), i, z));
            }
        }
        points.Add(new(end.x, end.y, z));
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    public static void DrawCurve(EdgeCollider2D line, Vector2 start, Vector2 end, string label, float radius, float precision)
    {
        line.edgeRadius = radius;
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
            for (float i = start.y; i < end.y; i += precision)
            {
                points.Add(new(CurveCalculator.CalcCurveCoord(start.x, end.x, (i - start.y) / (end.y - start.y), label), i));
            }
        }
        points.Add(new(end.x, end.y));
        line.points = points.ToArray();
    }

    /// <summary> 动态确定精度：在最高精度为0.1的前提下，最多将曲线分为100份 </summary>
    public static void DrawCurve(LineRenderer line, Vector2 start, Vector2 end, string label, float width)
    {
        float precision = Mathf.Max(0.1f, (end.y - start.y) / 100f);
        DrawCurve(line, start, end, label, width, precision);
    }

    /// <summary> 动态确定精度：在最高精度为0.2的前提下，最多将曲线分为100份 </summary>
    public static void DrawCurve(EdgeCollider2D line, Vector2 start, Vector2 end, string label, float radius)
    {
        float precision = Mathf.Max(0.1f, (end.y - start.y) / 100f);
        DrawCurve(line, start, end, label, radius, precision);
    }
}
