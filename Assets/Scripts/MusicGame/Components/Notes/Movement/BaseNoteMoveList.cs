using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using static Takana3.MusicGame.Values;

/// <summary>
/// 基本的记录Note垂直运动信息的列表，也可用于计算hold的长度
/// </summary>
public class BaseNoteMoveList : INoteMoveList
{
    // 还可以有CurveNoteMoveList、RandomNoteMoveList...?

    // Serializable and Public
    public int Count => moveList.Count;

    /// <summary> 表示该List是否已经初始化完全 </summary>
    public bool IsRaw => moveList == null;

    public (float time, float y) this[int index] => moveList[index];

    // Private
    private readonly List<(float time, float y)> rawMoveList = null; // 存下初始的列表，以便游戏中途修改速度时重新生成moveList
    private List<(float time, float y)> moveList = null;
    private readonly float timeStart = -TimePreAnimation; // Note超过TimeJudge时还应模拟其离开判定线的过程，因此不用记录TimeJudge
    private int lastIndex = 0;

    // Defined Functions
    public BaseNoteMoveList(float timeJudge, float speedRate = 1.0f, float timeStart = -TimePreAnimation) // 一般MoveList
    {
        rawMoveList = new()
        {
            (timeStart, speedRate * (timeJudge - timeStart)),
            (timeJudge, 0),
        };
        this.timeStart = timeStart;
    }

    public BaseNoteMoveList(List<(float time, float y)> moveList, float timeJudge, bool isRaw, float timeStart = -TimePreAnimation) // trail和speedtrail的MoveList
    {
        List<(float time, float y)> list = new(moveList);
        list.Insert(0, (timeStart, moveList[0].y));
        list.Add((timeJudge, 0));
        if (isRaw) rawMoveList = list;
        else this.moveList = list;
        this.timeStart = timeStart;
    }

    public BaseNoteMoveList(List<(float time, float speedRate)> speedList, float timeJudge, float timeStart = -TimePreAnimation) // speed的MoveList
    {
        speedList.Insert(0, (timeStart, speedList[0].speedRate));
        speedList.Add((timeJudge, 1));
        rawMoveList = new();
        float currentY = 0; // 倒序计算某时刻note所在位置
        rawMoveList.Add((timeJudge, currentY)); // TimeJudge时,note一定在判定线上
        for (int i = speedList.Count - 2; i >= 0; i--)
        {
            currentY += speedList[i].speedRate * (speedList[i + 1].time - speedList[i].time);
            rawMoveList.Add((speedList[i].time, currentY));
        }
        currentY += speedList[0].speedRate * (speedList[0].time - timeStart);
        rawMoveList.Add((timeStart, currentY));
        rawMoveList.Reverse(); // 把倒序顺过来
        this.timeStart = timeStart;
    }

    public Vector3 GetPos(float current)
    {
        Assert.IsFalse(IsRaw);
        if (current < timeStart) return new(0, moveList[0].y, 0);
        // 更新lastIndex的位置
        if (moveList[lastIndex].time > current) lastIndex = 0;
        while (lastIndex < moveList.Count - 2 && moveList[lastIndex + 1].time < current) lastIndex++;
        // 计算y值
        float departY = moveList[lastIndex].y, destY = moveList[lastIndex + 1].y;
        float t = (current - moveList[lastIndex].time) / (moveList[lastIndex + 1].time - moveList[lastIndex].time);
        return new(0, Mathf.LerpUnclamped(departY, destY, t), 0);
    }

    public void FixRaw(float speed)
    {
        if (rawMoveList == null) return; // 说明初始化时是直接初始化的moveList，即其与speed无关
        moveList = new(rawMoveList);
        float actualSpeed = ActualSpeed(speed);
        for (int i = 0; i < moveList.Count; i++)
        {
            float actualPos = actualSpeed * rawMoveList[i].y;
            moveList[i] = (rawMoveList[i].time, actualPos);
        }
    }

    public float CalcTimeInstantiate(float speed) // 该方法只在该List作为moveList时有意义
    {
        var actualSpeed = IsRaw ? ActualSpeed(speed) : 1.0f;
        var list = IsRaw ? rawMoveList : moveList;
        float ret = -TimePreAnimation;

        if (LowHeightLimit <= list[0].y && list[0].y <= UpHeightLimit) return list[0].time;

        for (int i = 0; i < list.Count - 1; i++)
        {
            float y1 = actualSpeed * list[i].y;
            float y2 = actualSpeed * list[i + 1].y;
            if (y1 >= UpHeightLimit && y2 < UpHeightLimit)
            {
                float t = (y1 - UpHeightLimit) / (y1 - y2);
                ret = Mathf.Lerp(list[i].time, list[i + 1].time, t);
                break;
            }
            else if (y1 <= LowHeightLimit && y2 > LowHeightLimit)
            {
                float t = (y1 - LowHeightLimit) / (y1 - y2);
                ret = Mathf.Lerp(list[i].time, list[i + 1].time, t);
                break;
            }
        }
        return ret;
    }
}
