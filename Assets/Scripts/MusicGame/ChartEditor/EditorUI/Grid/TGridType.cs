using UnityEngine;

public enum TGridType
{
    /// <summary> 四分音符（一拍） </summary>
    Beat,
    /// <summary> 八分音符 </summary>
    Quaver,
    /// <summary> 十六分音符 </summary>
    SemiQuaver,
    /// <summary> 各种三连音 </summary>
    Triplet,
    /// <summary> 其他情况 </summary>
    Default,
}

public static class TGridTypeExtension
{
    public static Color GetColor(this TGridType type)
    {
        return type switch
        {
            TGridType.Beat => new Color(0.7f, 0, 0),
            TGridType.Quaver => new Color(0.7f, 0.7f, 0),
            TGridType.SemiQuaver => new Color(0.7f, 0.7f, 0),
            TGridType.Triplet => new Color(0.7f, 0.4f, 0.7f),
            TGridType.Default or _ => new Color(0.8f, 0.8f, 0.8f),
        };
    }
}