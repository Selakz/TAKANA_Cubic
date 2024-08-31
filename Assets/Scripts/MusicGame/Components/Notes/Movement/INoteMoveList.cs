/// <summary>
/// 标识一个记录Note运动信息的列表
/// </summary>
public interface INoteMoveList : IMoveList
{
    public bool IsRaw { get; }

    /// <summary> 对列表进行初始化 </summary>
    public void FixRaw(float speed);

    /// <summary> 根据speed提供TimeInstantiate建议值 </summary>
    public float CalcTimeInstantiate(float speed);
}

