using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Ĭ�Ͽ糡���ĵ�����Ϣ��ȡ������SetInfo()���õ�����Ϣ����ReadInfo()��ȡ������Ϣ
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
            DontDestroyOnLoad(infoReader); // Ĭ��֧�ֿ糡��
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
