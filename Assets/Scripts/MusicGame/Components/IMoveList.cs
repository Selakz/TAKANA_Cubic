using UnityEngine;

/// <summary>
/// 标识一个记录元件运动信息的列表
/// </summary>
public interface IMoveList
{
    public int Count { get; }

    /// <summary> 获得当前时间的位置 </summary>
    public Vector3 GetPos(float current);
}