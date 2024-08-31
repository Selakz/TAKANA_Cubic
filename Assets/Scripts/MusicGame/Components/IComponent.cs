using Takana3.Settings;
using UnityEngine;

/// <summary>
/// 代表Playfield中的元件对应的信息
/// </summary>
public interface IComponent
{
    /// <summary> 元件的唯一标识 </summary>
    public int Id { get; }

    /// <summary> 标识该元件是否经过初始化，只应在调用<see cref="Initialize(MusicSetting)"/>后为真 </summary>
    public bool IsInitialized { get; }

    /// <summary> 如果生成了元件，由此获得其引用，否则该值为null </summary>
    public GameObject ThisObject { get; }

    /// <summary> 元件的生成时间，如果不需要则将其设为-TimePreAnimation </summary>
    public float TimeInstantiate { get; }

    // TODO: 改为返回状态码（bool不太够用）
    /// <summary>
    /// 尝试根据该元件的信息生成对应的<see cref="GameObject"/>，返回对应元件是否已生成；需在调用<see cref="Initialize(MusicSetting)"/>之后使用
    /// </summary>
    public bool Instantiate();

    /// <summary>
    /// 根据游戏设置对元件进行初始化。
    /// </summary>
    public void Initialize(MusicSetting setting);
}
