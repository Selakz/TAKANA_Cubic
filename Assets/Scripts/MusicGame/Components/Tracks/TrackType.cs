using System.IO;
using UnityEngine;

public enum TrackType
{
    /// <summary> 供UNote使用的透明轨道 </summary>
    Transparent,
    /// <summary> 一般轨道 </summary>
    Common,
}

public static class TrackTypeExtension
{
    public static readonly GameObject trackPrefab = MyResources.Load<GameObject>("Prefabs/Track");

    public static TrackType Char2TrackType(this TrackType trackType, char color)
    {
        return color switch
        {
            't' => TrackType.Transparent,
            _ => TrackType.Common,
        };
    }

    public static NoteType GetNoteType(this TrackType trackType, bool isHold = false)
    {
        return trackType switch
        {
            TrackType.Common => isHold ? NoteType.Hold : NoteType.Tap,
            _ => throw new System.Exception("Unhandled TrackType"),
        };
    }

    public static GameObject GetPrefab(this TrackType trackType)
    {
        return trackPrefab;
    }
}
