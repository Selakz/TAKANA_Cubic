using UnityEngine;

public enum TGridType
{
    /// <summary> �ķ�������һ�ģ� </summary>
    Beat,
    /// <summary> �˷����� </summary>
    Quaver,
    /// <summary> ʮ�������� </summary>
    SemiQuaver,
    /// <summary> ���������� </summary>
    Triplet,
    /// <summary> ������� </summary>
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