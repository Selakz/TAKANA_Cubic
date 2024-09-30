using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderMessage : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private Image panelImage;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Animator animator;

    public enum MessageType { Info, Warn, Error, Success }
    public static HeaderMessage Instance => _instance;

    // Private

    // Static
    private static HeaderMessage _instance;
    // TODO: 将要展示的信息用字典集成到该类中或集成到文件中

    // Defined Functions
    public static void Show(string message, MessageType type)
    {
        Instance.panelImage.color = type.GetColor();
        Instance.messageText.text = message;
        Instance.animator.Play(0);
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        if (ExceptionHandler.Instance != null)
        {
            ExceptionHandler.Instance.LogCallbacks.Add((string a, string b, LogType c) => Show("出现未处理异常，请将日志汇报给作者", MessageType.Error));
        }
    }
}

public static class MessageTypeExtension
{
    public static Color GetColor(this HeaderMessage.MessageType type)
    {
        return type switch
        {
            HeaderMessage.MessageType.Warn => new(1f, 1f, 0.4f, 0.7f),
            HeaderMessage.MessageType.Error => new(1f, 0.4f, 0.4f, 0.7f),
            HeaderMessage.MessageType.Success => new(0.4f, 1f, 0.4f, 0.7f),
            HeaderMessage.MessageType.Info or _ => new(0.4f, 0.7f, 1f, 0.7f),
        };
    }
}