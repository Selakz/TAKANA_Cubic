using System;
using static CurveCalculator;

public enum Eases
{
    Unmove, Linear,
    InSine, OutSine, InOutSine, OutInSine,
    InQuad, OutQuad, InOutQuad, OutInQuad,
    InCubic, OutCubic, InOutCubic, OutInCubic,
    InQuart, OutQuart, InOutQuart, OutInQuart,
    InQuint, OutQuint, InOutQuint, OutInQuint,
    InExpo, OutExpo, InOutExpo, OutInExpo,
    InCirc, OutCirc, InOutCirc, OutInCirc,
    InBack, OutBack, InOutBack, OutInBack,
    InElastic, OutElastic, InOutElastic, OutInElastic,
    InBounce, OutBounce, InOutBounce, OutInBounce,
}

/// <summary>
/// Provide some calculation method of easing curves.
/// </summary>
public static class CurveCalculator
{
    /// <summary> 全局控制计算坐标时是否取反（<see cref="EasesExtension.Opposite(Eases)"/>） </summary>
    public static bool IsReverse { get; set; } = true;

    public static Eases GetEaseByName(string easeName)
    {
        foreach (Eases ease in Enum.GetValues(typeof(Eases)))
        {
            if (ease.GetString() == easeName) return ease;
        }
        throw new Exception();
    }

    public static Eases GetEaseByRpeNumber(int rpeNumber)
    {
        foreach (Eases ease in Enum.GetValues(typeof(Eases)))
        {
            if (ease.GetRpeNumber() == rpeNumber) return ease;
        }
        throw new Exception();
    }

    public static Eases GetEaseById(int easeId)
    {
        if (easeId < 0 || easeId > 101) throw new ArgumentOutOfRangeException();
        if (easeId % 10 == 0) return Eases.Unmove;
        if (easeId % 10 == 1) return Eases.Linear;
        foreach (Eases ease in Enum.GetValues(typeof(Eases)))
        {
            if (ease.GetId() == easeId) return ease;
        }
        throw new Exception();
    }

    #region EachCalcCurveMethod
    public static float EaseLinear(float left, float right, float t)
    {
        return left + (right - left) * t;
    }

    public static float EaseStill(float left, float right, float t)
    {
        return left;
    }

    // Sine
    public static float EaseInSine(float left, float right, float t)
    {
        return left + (right - left) * (float)(1 - Math.Cos((t * Math.PI) / 2));
    }

    public static float EaseOutSine(float left, float right, float t)
    {
        return left + (right - left) * (float)Math.Sin((t * Math.PI) / 2);
    }

    public static float EaseInOutSine(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInSine(left, (left + right) / 2, t * 2);
        else
            return EaseOutSine((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInSine(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutSine(left, (left + right) / 2, t * 2);
        else
            return EaseInSine((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Quad
    public static float EaseInQuad(float left, float right, float t)
    {
        return left + (right - left) * t * t;
    }

    public static float EaseOutQuad(float left, float right, float t)
    {
        return left + (right - left) * (1 - (1 - t) * (1 - t));
    }

    public static float EaseInOutQuad(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInQuad(left, (left + right) / 2, t * 2);
        else
            return EaseOutQuad((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInQuad(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutQuad(left, (left + right) / 2, t * 2);
        else
            return EaseInQuad((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Cubic
    public static float EaseInCubic(float left, float right, float t)
    {
        return left + (right - left) * t * t * t;
    }

    public static float EaseOutCubic(float left, float right, float t)
    {
        return left + (right - left) * (1 - (1 - t) * (1 - t) * (1 - t));
    }

    public static float EaseInOutCubic(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInCubic(left, (left + right) / 2, t * 2);
        else
            return EaseOutCubic((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInCubic(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutCubic(left, (left + right) / 2, t * 2);
        else
            return EaseInCubic((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Quart
    public static float EaseInQuart(float left, float right, float t)
    {
        return left + (right - left) * t * t * t * t;
    }

    public static float EaseOutQuart(float left, float right, float t)
    {
        return left + (right - left) * (1 - (1 - t) * (1 - t) * (1 - t) * (1 - t));
    }

    public static float EaseInOutQuart(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInQuart(left, (left + right) / 2, t * 2);
        else
            return EaseOutQuart((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInQuart(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutQuart(left, (left + right) / 2, t * 2);
        else
            return EaseInQuart((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Quint
    public static float EaseInQuint(float left, float right, float t)
    {
        return left + (right - left) * t * t * t * t * t;
    }

    public static float EaseOutQuint(float left, float right, float t)
    {
        return left + (right - left) * (1 - (1 - t) * (1 - t) * (1 - t) * (1 - t) * (1 - t));
    }

    public static float EaseInOutQuint(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInQuint(left, (left + right) / 2, t * 2);
        else
            return EaseOutQuint((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInQuint(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutQuint(left, (left + right) / 2, t * 2);
        else
            return EaseInQuint((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Expo
    public static float EaseInExpo(float left, float right, float t)
    {
        return left + (right - left) * (t == 0 ? 0 : (float)Math.Pow(2, 10 * (t - 1)));
    }

    public static float EaseOutExpo(float left, float right, float t)
    {
        return left + (right - left) * (t == 1 ? 1 : 1 - (float)Math.Pow(2, -10 * t));
    }

    public static float EaseInOutExpo(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInExpo(left, (left + right) / 2, t * 2);
        else
            return EaseOutExpo((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInExpo(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutExpo(left, (left + right) / 2, t * 2);
        else
            return EaseInExpo((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Circ
    public static float EaseInCirc(float left, float right, float t)
    {
        return left + (right - left) * (1 - (float)Math.Sqrt(1 - t * t));
    }

    public static float EaseOutCirc(float left, float right, float t)
    {
        return left + (right - left) * (float)Math.Sqrt(1 - (t - 1) * (t - 1));
    }

    public static float EaseInOutCirc(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInCirc(left, (left + right) / 2, t * 2);
        else
            return EaseOutCirc((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInCirc(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutCirc(left, (left + right) / 2, t * 2);
        else
            return EaseInCirc((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Back
    public static float EaseInBack(float left, float right, float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return left + (right - left) * (c3 * t * t * t - c1 * t * t);
    }

    public static float EaseOutBack(float left, float right, float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1;
        return left + (right - left) * (1 + c3 * (t - 1) * (t - 1) * (t - 1) + c1 * (t - 1) * (t - 1));
    }

    public static float EaseInOutBack(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInBack(left, (left + right) / 2, t * 2);
        else
            return EaseOutBack((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInBack(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutBack(left, (left + right) / 2, t * 2);
        else
            return EaseInBack((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Elastic
    public static float EaseInElastic(float left, float right, float t)
    {
        float c4 = (2 * (float)Math.PI) / 3;
        return left + (right - left) * (t == 0 ? 0 : t == 1 ? 1 : -(float)Math.Pow(2, 10 * t - 10) * (float)Math.Sin((t * 10 - 10.75f) * c4));
    }

    public static float EaseOutElastic(float left, float right, float t)
    {
        float c4 = (2 * (float)Math.PI) / 3;
        return left + (right - left) * (t == 0 ? 0 : t == 1 ? 1 : (float)Math.Pow(2, -10 * t) * (float)Math.Sin((t * 10 - 0.75f) * c4) + 1);
    }

    public static float EaseInOutElastic(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInElastic(left, (left + right) / 2, t * 2);
        else
            return EaseOutElastic((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInElastic(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutElastic(left, (left + right) / 2, t * 2);
        else
            return EaseInElastic((left + right) / 2, right, (t - 0.5f) * 2);
    }

    // Bounce

    public static float EaseInBounce(float left, float right, float t)
    {
        return left + (right - left) * (1 - EaseOutBounce(0, 1, 1 - t));
    }

    public static float EaseOutBounce(float left, float right, float t)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;
        if (t < 1 / d1)
            return left + (right - left) * (n1 * t * t);
        else if (t < 2 / d1)
            return left + (right - left) * (n1 * (t -= 1.5f / d1) * t + 0.75f);
        else if (t < 2.5 / d1)
            return left + (right - left) * (n1 * (t -= 2.25f / d1) * t + 0.9375f);
        else
            return left + (right - left) * (n1 * (t -= 2.625f / d1) * t + 0.984375f);
    }

    public static float EaseInOutBounce(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseInBounce(left, (left + right) / 2, t * 2);
        else
            return EaseOutBounce((left + right) / 2, right, (t - 0.5f) * 2);
    }

    public static float EaseOutInBounce(float left, float right, float t)
    {
        if (t < 0.5f)
            return EaseOutBounce(left, (left + right) / 2, t * 2);
        else
            return EaseInBounce((left + right) / 2, right, (t - 0.5f) * 2);
    }

    #endregion

}

public static class EasesExtension
{
    public static Eases Opposite(this Eases e)
    {
        return e switch
        {
            Eases.Unmove => Eases.Unmove,
            Eases.Linear => Eases.Linear,
            Eases.InSine => Eases.OutSine,
            Eases.OutSine => Eases.InSine,
            Eases.InOutSine => Eases.OutInSine,
            Eases.OutInSine => Eases.InOutSine,
            Eases.InQuad => Eases.OutQuad,
            Eases.OutQuad => Eases.InQuad,
            Eases.InOutQuad => Eases.OutInQuad,
            Eases.OutInQuad => Eases.InOutQuad,
            Eases.InCubic => Eases.OutCubic,
            Eases.OutCubic => Eases.InCubic,
            Eases.InOutCubic => Eases.OutInCubic,
            Eases.OutInCubic => Eases.InOutCubic,
            Eases.InQuart => Eases.OutQuart,
            Eases.OutQuart => Eases.InQuart,
            Eases.InOutQuart => Eases.OutInQuart,
            Eases.OutInQuart => Eases.InOutQuart,
            Eases.InQuint => Eases.OutQuint,
            Eases.OutQuint => Eases.InQuint,
            Eases.InOutQuint => Eases.OutInQuint,
            Eases.OutInQuint => Eases.InOutQuint,
            Eases.InExpo => Eases.OutExpo,
            Eases.OutExpo => Eases.InExpo,
            Eases.InOutExpo => Eases.OutInExpo,
            Eases.OutInExpo => Eases.InOutExpo,
            Eases.InCirc => Eases.OutCirc,
            Eases.OutCirc => Eases.InCirc,
            Eases.InOutCirc => Eases.OutInCirc,
            Eases.OutInCirc => Eases.InOutCirc,
            Eases.InBack => Eases.OutBack,
            Eases.OutBack => Eases.InBack,
            Eases.InOutBack => Eases.OutInBack,
            Eases.OutInBack => Eases.InOutBack,
            Eases.InElastic => Eases.OutElastic,
            Eases.OutElastic => Eases.InElastic,
            Eases.InOutElastic => Eases.OutInElastic,
            Eases.OutInElastic => Eases.InOutElastic,
            Eases.InBounce => Eases.OutBounce,
            Eases.OutBounce => Eases.InBounce,
            Eases.InOutBounce => Eases.OutInBounce,
            Eases.OutInBounce => Eases.InOutBounce,
            _ => throw new NotImplementedException(),
        };
    }

    public static int GetRpeNumber(this Eases e)
    {
        return e switch
        {
            Eases.Unmove => 0,
            Eases.Linear => 1,
            Eases.InSine => 3,
            Eases.OutSine => 2,
            Eases.InOutSine => 6,
            Eases.OutInSine => 32,
            Eases.InQuad => 5,
            Eases.OutQuad => 4,
            Eases.InOutQuad => 7,
            Eases.OutInQuad => 33,
            Eases.InCubic => 9,
            Eases.OutCubic => 8,
            Eases.InOutCubic => 12,
            Eases.OutInCubic => 34,
            Eases.InQuart => 11,
            Eases.OutQuart => 10,
            Eases.InOutQuart => 13,
            Eases.OutInQuart => 35,
            Eases.InQuint => 15,
            Eases.OutQuint => 14,
            Eases.InOutQuint => 30,
            Eases.OutInQuint => 36,
            Eases.InExpo => 17,
            Eases.OutExpo => 16,
            Eases.InOutExpo => 31,
            Eases.OutInExpo => 37,
            Eases.InCirc => 19,
            Eases.OutCirc => 18,
            Eases.InOutCirc => 22,
            Eases.OutInCirc => 38,
            Eases.InBack => 21,
            Eases.OutBack => 20,
            Eases.InOutBack => 23,
            Eases.OutInBack => 39,
            Eases.InElastic => 25,
            Eases.OutElastic => 24,
            Eases.InOutElastic => 29,
            Eases.OutInElastic => 40,
            Eases.InBounce => 27,
            Eases.OutBounce => 26,
            Eases.InOutBounce => 28,
            Eases.OutInBounce => 41,
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }

    public static int GetEaseId(this Eases e)
    {
        int enumId = (int)e;
        if (enumId == 0 || enumId == 1) return 10;
        else return ((enumId - 2) / 4 + 1) % 10; // 对应键盘上的数字键从1到0，同时也使幂函数的次数即为其缓动ID
    }

    public static int GetMoveId(this Eases e)
    {
        int enumId = (int)e;
        if (enumId == 0 || enumId == 1) return enumId;
        else return ((enumId - 2) % 4 + 2); // u 0; s 1; In 2; Out 3; InOut 4; OutIn 5;
    }

    public static int GetId(this Eases e)
    {
        if (e == Eases.Unmove) return 100;
        if (e == Eases.Linear) return 101;
        return e.GetEaseId() * 10 + e.GetMoveId();
    }

    public static string GetString(this Eases e)
    {
        return e switch
        {
            Eases.Unmove => "u",
            Eases.Linear => "s",
            Eases.InSine => "si",
            Eases.OutSine => "so",
            Eases.InOutSine => "sa",
            Eases.OutInSine => "sb",
            Eases.InQuad => "2i",
            Eases.OutQuad => "2o",
            Eases.InOutQuad => "2a",
            Eases.OutInQuad => "2b",
            Eases.InCubic => "3i",
            Eases.OutCubic => "3o",
            Eases.InOutCubic => "3a",
            Eases.OutInCubic => "3b",
            Eases.InQuart => "4i",
            Eases.OutQuart => "4o",
            Eases.InOutQuart => "4a",
            Eases.OutInQuart => "4b",
            Eases.InQuint => "5i",
            Eases.OutQuint => "5o",
            Eases.InOutQuint => "5a",
            Eases.OutInQuint => "5b",
            Eases.InExpo => "ei",
            Eases.OutExpo => "eo",
            Eases.InOutExpo => "ea",
            Eases.OutInExpo => "eb",
            Eases.InCirc => "ci",
            Eases.OutCirc => "co",
            Eases.InOutCirc => "ca",
            Eases.OutInCirc => "cb",
            Eases.InBack => "bi",
            Eases.OutBack => "bo",
            Eases.InOutBack => "ba",
            Eases.OutInBack => "bb",
            Eases.InElastic => "li",
            Eases.OutElastic => "lo",
            Eases.InOutElastic => "la",
            Eases.OutInElastic => "lb",
            Eases.InBounce => "wi",
            Eases.OutBounce => "wo",
            Eases.InOutBounce => "wa",
            Eases.OutInBounce => "wb",
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
    }

    /// <summary> 不受全局变量<see cref="IsReverse"/>影响的计算坐标方法 </summary>
    public static float CalcCoord(this Eases e, float left, float right, float t, bool isReverse)
    {
        if (isReverse) e = e.Opposite();
        Func<float, float, float, float> func = e switch
        {
            Eases.Unmove => EaseStill,
            Eases.Linear => EaseLinear,
            Eases.InSine => EaseInSine,
            Eases.OutSine => EaseOutSine,
            Eases.InOutSine => EaseInOutSine,
            Eases.OutInSine => EaseOutInSine,
            Eases.InQuad => EaseInQuad,
            Eases.OutQuad => EaseOutQuad,
            Eases.InOutQuad => EaseInOutQuad,
            Eases.OutInQuad => EaseOutInQuad,
            Eases.InCubic => EaseInCubic,
            Eases.OutCubic => EaseOutCubic,
            Eases.InOutCubic => EaseInOutCubic,
            Eases.OutInCubic => EaseOutInCubic,
            Eases.InQuart => EaseInQuart,
            Eases.OutQuart => EaseOutQuart,
            Eases.InOutQuart => EaseInOutQuart,
            Eases.OutInQuart => EaseOutInQuart,
            Eases.InQuint => EaseInQuint,
            Eases.OutQuint => EaseOutQuint,
            Eases.InOutQuint => EaseInOutQuint,
            Eases.OutInQuint => EaseOutInQuint,
            Eases.InExpo => EaseInExpo,
            Eases.OutExpo => EaseOutExpo,
            Eases.InOutExpo => EaseInOutExpo,
            Eases.OutInExpo => EaseOutInExpo,
            Eases.InCirc => EaseInCirc,
            Eases.OutCirc => EaseOutCirc,
            Eases.InOutCirc => EaseInOutCirc,
            Eases.OutInCirc => EaseOutInCirc,
            Eases.InBack => EaseInBack,
            Eases.OutBack => EaseOutBack,
            Eases.InOutBack => EaseInOutBack,
            Eases.OutInBack => EaseOutInBack,
            Eases.InElastic => EaseInElastic,
            Eases.OutElastic => EaseOutElastic,
            Eases.InOutElastic => EaseInOutElastic,
            Eases.OutInElastic => EaseOutInElastic,
            Eases.InBounce => EaseInBounce,
            Eases.OutBounce => EaseOutBounce,
            Eases.InOutBounce => EaseInOutBounce,
            Eases.OutInBounce => EaseOutInBounce,
            _ => throw new ArgumentOutOfRangeException(nameof(e), e, null)
        };
        return func(left, right, t);
    }

    public static float CalcCoord(this Eases e, float left, float right, float t)
    {
        return e.CalcCoord(left, right, t, IsReverse);
    }
}
