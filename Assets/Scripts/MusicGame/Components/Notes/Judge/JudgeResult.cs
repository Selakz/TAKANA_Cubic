using System.Collections.Generic;

public enum JudgeResult
{
    NotHit,
    EarlyMiss,
    EarlyBad,
    EarlyGreat,
    EarlyPerfect,
    CriticalPerfect,
    LatePerfect,
    LateGreat,
    LateBad,
    LateMiss,
}

public static class JudgeResultExtension
{
    public static List<JudgeResult> BreakCombo(this JudgeResult result)
    {
        return new()
        {
            JudgeResult.EarlyMiss,
            JudgeResult.EarlyBad,
            JudgeResult.LateBad,
            JudgeResult.LateMiss,
        };
    }
}