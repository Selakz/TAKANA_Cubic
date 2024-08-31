/// <summary>
/// 标识某个元件属于Track类
/// </summary>
public interface ITrack : IComponent
{
    /// <summary> Track的类型 </summary>
    public TrackType Type { get; }

    /// <summary> Track的消失时间 </summary>
    public float TimeEnd { get; }

    /// <summary> Track是否可见 </summary>
    public bool IsVisible { get; }

    /// <summary> Track生成时是否播放生成动画 </summary>
    public bool IsPreAnimate { get; }

    /// <summary> Track消失时是否播放消失动画 </summary>
    public bool IsPostAnimate { get; }
}
