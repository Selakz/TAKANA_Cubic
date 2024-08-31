/// <summary>
/// 标识Note的判定类型，同类型的Note接收同种输入，一般情况下与TrackType对应。
/// </summary>
public enum JudgeType
{
    /// <summary> 无轨音符判定 </summary>
    UnTrack,
    /// <summary> 蓝色音符判定 </summary>
    Common,
    /// <summary> 红色音符判定 </summary>
    Red,
    /// <summary> 暗色音符判定 </summary>
    Dark,
}

