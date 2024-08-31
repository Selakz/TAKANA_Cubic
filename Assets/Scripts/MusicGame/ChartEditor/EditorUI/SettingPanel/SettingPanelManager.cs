using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanelManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] Toggle isCopyToClipboardAllowedToggle;
    [SerializeField] Toggle isAutoSaveAllowedToggle;
    [SerializeField] TMP_InputField autoSaveIntervalInputField;
    [SerializeField] private List<Selectable> selectables;

    public static SettingPanelManager Instance => _instance;

    // Private
    private int autoSaveinterval;

    // Static
    private static SettingPanelManager _instance;

    // Defined Functions
    public void ReadSettings()
    {
        var setting = EditingLevelManager.Instance.GlobalSetting;
        isCopyToClipboardAllowedToggle.isOn = setting.IsCopyToClipboardAllowed;
        isAutoSaveAllowedToggle.isOn = setting.IsAutoSaveAllowed;
        autoSaveinterval = setting.AutoSaveInterval_Minute;
        autoSaveIntervalInputField.text = setting.AutoSaveInterval_Minute.ToString();
    }

    public void ToggleCopyToClipboardAllowed()
    {
        EditingLevelManager.Instance.GlobalSetting.IsCopyToClipboardAllowed = isCopyToClipboardAllowedToggle.isOn;
    }

    public void ToggleAutoSaveAllowed()
    {
        EditingLevelManager.Instance.GlobalSetting.IsAutoSaveAllowed = isAutoSaveAllowedToggle.isOn;
    }

    public void OnAutoSaveIntervalEndEdit()
    {
        if (int.TryParse(autoSaveIntervalInputField.text, out int newInterval))
        {
            if (newInterval > 0)
            {
                EditingLevelManager.Instance.GlobalSetting.AutoSaveInterval_Minute = newInterval;
                return;
            }

        }
        autoSaveIntervalInputField.text = autoSaveinterval.ToString();
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void OnEnable()
    {
        ReadSettings();
        foreach (var item in selectables)
        {
            item.interactable = true;
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
