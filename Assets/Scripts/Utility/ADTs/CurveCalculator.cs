using System;
using System.Collections.Generic;

/// <summary>
/// Provide some calculation method of easing curves.
/// </summary>
public static class CurveCalculator
{
    public static List<string> Labels => new()
    {
        // Linear (s = straight, u = unmove)
        "s", "u",
        // Sine curves
        "si", "so", "sb", "sa", 
        // Circular curves
        "ci", "co", "cb", "ca",
        // Exponential curves
        "ei", "eo", "eb", "ea",
    };

    public static string LabelsRegex
    {
        get
        {
            string ret = @"(?:";
            foreach (var label in Labels)
            {
                ret += label + '|';
            }
            ret.Remove(ret.Length - 1);
            ret += ')';
            return ret;
        }
    }

    /// <summary>
    /// Using the provided two edge to calculate the y value corresponding to x, where t = (x - left) / (right - left).<br/>
    /// </summary>
    public static float CalcCurveCoord(float left, float right, float t, string curveLabel)
    {
        // TODO: 其他类型曲线计算
        Func<float, float, float, float> func =
        curveLabel switch
        {
            "u" => EaseStill,
            "s" => EaseLinear,
            "si" => EaseInSine,
            "so" => EaseOutSine,
            "sb" => EaseOutInSine,
            "sa" => EaseInOutSine,
            "ei" => EaseInExpo,
            "eo" => EaseOutExpo,
            "eb" => EaseOutInExpo,
            "ea" => EaseInOutExpo,
            "ci" => EaseInCirc,
            "co" => EaseOutCirc,
            "cb" => EaseOutInCirc,
            "ca" => EaseInOutCirc,
            _ => EaseStill,
        };
        return func(left, right, t);
    }

    public static float EaseLinear(float left, float right, float t)
    {
        return left + (right - left) * t;
    }

    public static float EaseStill(float left, float right, float t)
    {
        return left;
    }

    public static float EaseOutSine(float left, float right, float t)
    {
        return left + (right - left) * (float)(1 - Math.Cos((t * Math.PI) / 2));
    }

    public static float EaseInSine(float left, float right, float t)
    {
        return left + (right - left) * (float)Math.Sin((t * Math.PI) / 2);
    }

    public static float EaseOutInSine(float left, float right, float t)
    {
        return left + (right - left) * -(float)(Math.Cos(Math.PI * t) - 1) / 2;
    }

    public static float EaseInOutSine(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInSine(left, (left + right) / 2, t * 2);
        else
            return EaseOutSine((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutExpo(float left, float right, float t)
    {
        return left + (right - left) * (float)(t == 0 ? 0 : Math.Pow(2, 10 * (t - 1)));
    }

    public static float EaseInExpo(float left, float right, float t)
    {
        return left + (right - left) * (float)(t == 1 ? 1 : 1 - Math.Pow(2, -10 * t));
    }

    public static float EaseOutInExpo(float left, float right, float t)
    {
        if (t == 0) return left;
        if (t == 1) return right;
        if (t < 0.5f)
            return left + (right - left) * (float)Math.Pow(2, 20 * t - 10) / 2;
        else
            return left + (right - left) * (float)(2 - Math.Pow(2, -20 * t + 10)) / 2;
    }

    public static float EaseInOutExpo(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutExpo(left, (left + right) / 2, t * 2);
        else
            return EaseInExpo((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutCirc(float left, float right, float t)
    {
        return left + (right - left) * (float)(1 - Math.Sqrt(1 - t * t));
    }

    public static float EaseInCirc(float left, float right, float t)
    {
        return left + (right - left) * (float)Math.Sqrt(1 - Math.Pow(t - 1, 2));
    }

    public static float EaseOutInCirc(float left, float right, float t)
    {
        if (t < 0.5f)
            return left + (right - left) * (float)(1 - Math.Sqrt(1 - 4 * t * t)) / 2;
        else
            return left + (right - left) * (float)(Math.Sqrt(1 - Math.Pow(-2 * t + 2, 2)) + 1) / 2;
    }

    public static float EaseInOutCirc(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutCirc(left, (left + right) / 2, t * 2);
        else
            return EaseInCirc((left + right) / 2, right, (t - 0.5f) * 2);
    }
}
