using System.ComponentModel;
using UnityEngine;

/// <summary>
/// 默认跨场景的单例信息读取器。用SetInfo()设置单例信息，用ReadInfo()获取单例信息
/// </summary>
public class InfoReader : MonoBehaviour
{
    // Serializable and Public
    public static InfoReader Instance => _instance;
    private static InfoReader _instance;

    // Private
    private IPassInfo info = null;

    // Defined Function
    public static void SetInfo(IPassInfo info)
    {
        if (_instance == null)
        {
            GameObject infoReader = new() { name = "InfoReader" };
            _instance = infoReader.AddComponent<InfoReader>();
            DontDestroyOnLoad(infoReader); // 默认支持跨场景
        }
        _instance.info = info;
    }

    public static I ReadInfo<I>() where I : IPassInfo
    {
        if (_instance == null || _instance.info is not I)
        {
            Debug.LogWarning("Invalid read operation.");
            return default;
        }
        return (I)_instance.info;
    }
}
