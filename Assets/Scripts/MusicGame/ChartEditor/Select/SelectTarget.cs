using System;
using UnityEngine;

public enum SelectTarget
{
    Note,
    Track,
    TurningPoint,
    Others,
}

public static class SelectTargetExtension
{
    private static readonly LayerMask TrackLayer = LayerMask.GetMask("Tracks");
    private static readonly LayerMask NoteLayer = LayerMask.GetMask("Notes");
    private static readonly LayerMask TurningPointLayer = LayerMask.GetMask("TurningPoints");

    public static LayerMask GetMask(this SelectTarget selectTarget)
    {
        return selectTarget switch
        {
            SelectTarget.Note => NoteLayer,
            SelectTarget.Track => TrackLayer,
            SelectTarget.TurningPoint => TurningPointLayer,
            _ => throw new Exception("Unhandled SelectTarget"),
        };
    }
}