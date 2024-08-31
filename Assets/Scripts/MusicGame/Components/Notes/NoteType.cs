using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// T3官方给三种Note的命名是Note、Hold和Long，不过我并不喜欢，所以这里还是按照更一般接受的命名来了。
/// </summary>
public enum NoteType
{
    /// <summary> 无轨单点 </summary>
    UTap,
    /// <summary> 无轨长按 </summary>
    UHold,
    /// <summary> 无轨滑动 </summary>
    USlide,
    /// <summary> 有轨单点 </summary>
    Tap,
    /// <summary> 有轨长按 </summary>
    Hold,
    /// <summary> 有轨滑动 </summary>
    Slide,
}

public static class NoteTypeExtension
{
    public static List<NoteType> UNoteTypes(this NoteType noteType)
    {
        return new List<NoteType>() {
            NoteType.UTap,
            NoteType.UHold,
            NoteType.USlide,
        };
    }

    public static List<NoteType> TNoteTypes(this NoteType noteType)
    {
        return new List<NoteType> {
            NoteType.Tap,
            NoteType.Hold,
            NoteType.Slide,
        };
    }

    // TODO: 修改不同Note的JudgeInfo
    public static JudgeInfo GetJudgeInfo(this NoteType noteType)
    {
        string pathPart = "";

        string path = $"ScriptableObject/{pathPart}JudgeInfo";
        return MyResources.Load<JudgeInfo>(path);
    }

    public static JudgeType GetJudgeType(this NoteType noteType)
    {
        return noteType switch
        {
            NoteType.UTap or NoteType.UHold or NoteType.USlide => JudgeType.UnTrack,
            NoteType.Tap or NoteType.Hold or NoteType.Slide => JudgeType.Common,
            _ => throw new Exception("Unhandled NoteType"),
        };
    }

    public static GameObject GetGameObject(this NoteType noteType)
    {
        string pathPart = noteType switch
        {
            NoteType.UTap => "UntrackTap",
            // NoteType.UHold =>
            // NoteType.USlide =>
            NoteType.Tap => "Tap",
            NoteType.Hold => "Hold",
            NoteType.Slide => "Slide",
            _ => throw new Exception("Unhandled NoteType"),
        };

        string path = $"Prefabs/{pathPart}";
        return MyResources.Load<GameObject>(path);
    }
}