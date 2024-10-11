using Takana3.Settings;
using TMPro;
using UnityEngine;

// TODO: ��һ�������˰�����������ŵ�����һ��Manager�����Ҫ�ԡ�����
public class ResolutionDropdown : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private TMP_Dropdown dropdown;

    // Private

    // Static

    // Defined Functions
    public void OnResolutionValueChanged()
    {
        var newResolutionText = dropdown.options[dropdown.value].text;
        var (width, height) = ParseResolution(newResolutionText);
        Screen.SetResolution(width, height, false);
        var setting = EditorGlobalSetting.Load();
        setting.Resolution = newResolutionText;
        setting.Save();
        HeaderMessage.Show("�л��ֱ��ʳɹ���", HeaderMessage.MessageType.Success);
    }

    private static (int w, int h) ParseResolution(string resolutionText)
    {
        string[] split = resolutionText.Split('x');
        return (int.Parse(split[0]), int.Parse(split[1]));
    }

    // System Functions
    void Start()
    {
        var resolutionText = EditorGlobalSetting.Load().Resolution;
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            if (dropdown.options[i].text == resolutionText)
            {
                dropdown.SetValueWithoutNotify(i);
                break;
            }
        }
        var (width, height) = ParseResolution(resolutionText);
        if (width == Screen.currentResolution.width && height == Screen.currentResolution.height) return;
        else Screen.SetResolution(width, height, false);
    }
}
