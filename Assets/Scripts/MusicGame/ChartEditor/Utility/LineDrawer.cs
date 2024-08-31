using System.Collections.Generic;
using UnityEngine;

// ¼òÒ×»­Ïß
public static class LineDrawer
{
    public static void DrawCurve(LineRenderer line, Vector2 start, Vector2 end, string label, float width, float precision)
    {
        line.startWidth = line.endWidth = width;
        float z = -1f;
        List<Vector3> points = new();
        for (float i = start.y; i < end.y; i += precision)
        {
            points.Add(new(CurveCalculator.CalcCurveCoord(start.x, end.x, (i - start.y) / (end.y - start.y), label), i, z));
        }
        points.Add(new(end.x, end.y, z));
        line.positionCount = points.Count;
        line.SetPositions(points.ToArray());
    }

    public static void DrawCurve(EdgeCollider2D line, Vector2 start, Vector2 end, string label, float radius, float precision)
    {
        line.edgeRadius = radius;
        List<Vector2> points = new();
        for (float i = start.y; i < end.y; i += precision)
        {
            points.Add(new(CurveCalculator.CalcCurveCoord(start.x, end.x, (i - start.y) / (end.y - start.y), label), i));
        }
        points.Add(new(end.x, end.y));
        line.points = points.ToArray();
    }
}
