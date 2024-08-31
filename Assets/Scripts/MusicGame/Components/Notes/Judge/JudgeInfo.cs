using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示一个判定区间。输入偏差的计算应为TimeJudge - TimeInput，该值为正，说明按早了，反之说明按晚了
/// </summary>
[CreateAssetMenu(fileName = "JudgeInfo", menuName = "ScriptableObjects/JudgeInfo", order = 0)]
public class JudgeInfo : ScriptableObject
{
    /// <summary> 获得某一判定结果对应的得分比率 </summary>
    public float this[JudgeResult judgeResult]
    {
        get
        {
            foreach (var range in judgeRanges)
            {
                if (range.judgeResult == judgeResult)
                {
                    return range.scoreRate;
                }
            }
            return 0;
        }
    }

    /// <summary> 在一般道义下，该值应为正数 </summary>
    public float TimeMiss;

    [SerializeField] private List<JudgeRange> judgeRanges;

    public JudgeResult GetJudge(float deviation)
    {
        foreach (JudgeRange range in judgeRanges)
        {
            if (range.Contains(deviation))
            {
                return range.judgeResult;
            }
        }
        return JudgeResult.NotHit;
    }

    [System.Serializable]
    private class JudgeRange
    {
        public JudgeResult judgeResult;

        /// <summary> 该判定区间的最早边界（包含） </summary>
        public float earlyEdge;

        /// <summary> 该判定区间的最晚边界（不包含） </summary>
        public float lateEdge;

        /// <summary> 该判定下的得分比率 </summary>
        public float scoreRate;

        public bool Contains(float deviation)
        {
            return lateEdge < deviation && deviation <= earlyEdge;
        }
    }
}
