/// <summary>
/// 标识某个元件属于Note类：即通过TimeJudge进行判定
/// </summary>
public interface INote : IComponent
{
    /// <summary> Note的类型 </summary>
    public NoteType Type { get; }

    /// <summary> Note的判定类型 </summary>
    public JudgeType JudgeType { get; }

    /// <summary> Note的基准判定时间 </summary>
    public float TimeJudge { get; }

    /// <summary> Note的所有判定区间 </summary>
    public JudgeInfo JudgeInfo { get; }

    /// <summary> Note的输入信息：在关卡初始化时设定 </summary>
    public InputInfo InputInfo { get; set; }

    /// <summary> 处理输入，并返回是否判定成功；一般将该处理逻辑全部放到对应Controller中 </summary>
    public bool HandleInput(float timeInput);

    public InputInfo GetInput();
}
