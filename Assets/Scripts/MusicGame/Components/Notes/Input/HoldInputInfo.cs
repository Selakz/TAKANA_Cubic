using System.Collections.Generic;

// 健壮性什么的先歇一歇吧
public class HoldInputInfo : InputInfo
{
    /// <summary> 请保证是有序递增的区间 </summary>
    public List<(float start, float end)> ReleaseTimes { get; set; }

    private int lastIndex = 0;

    public bool IsRelease(float current)
    {
        if (ReleaseTimes.Count == 0) return false;
        // 更新index的位置直到小于某个区间右端
        if (ReleaseTimes[lastIndex].start > current) lastIndex = 0;
        while (lastIndex < ReleaseTimes.Count && ReleaseTimes[lastIndex].end < current) lastIndex++;
        // 如果current还小于该区间左端，说明已经松开
        if (lastIndex >= ReleaseTimes.Count || ReleaseTimes[lastIndex].start > current) return true;
        else return false;
    }
}
