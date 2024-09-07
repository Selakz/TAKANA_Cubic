using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanelManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private Toggle isCopyToClipboardAllowedToggle;
    [SerializeField] private Toggle isAutoSaveAllowedToggle;
    [SerializeField] private TMP_InputField autoSaveIntervalInputField;
    [SerializeField] private Toggle isForcePauseAllowedToggle;
    [SerializeField] private Toggle isInitialTrackLengthNotToEndToggle;
    [SerializeField] private TMP_InputField initialTrackLengthInputField;
    [SerializeField] private List<Selectable> selectables;

    public static SettingPanelManager Instance => _instance;

    // Private
    private int autoSaveinterval;
    private int initialTrackLength;

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
        isForcePauseAllowedToggle.isOn = setting.IsForcePauseAllowed;
        isInitialTrackLengthNotToEndToggle.isOn = setting.IsInitialTrackLengthNotToEnd;
        initialTrackLength = setting.InitialTrackLength_Ms;
        initialTrackLengthInputField.text = setting.InitialTrackLength_Ms.ToString();
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
                autoSaveinterval = newInterval;
                EditingLevelManager.Instance.GlobalSetting.AutoSaveInterval_Minute = newInterval;
                return;
            }

        }
        autoSaveIntervalInputField.text = autoSaveinterval.ToString();
    }

    public void ToggleForcePauseAllowed()
    {
        EditingLevelManager.Instance.GlobalSetting.IsForcePauseAllowed = isForcePauseAllowedToggle.isOn;
    }

    public void ToggleInitialTrackLengthToEnd()
    {
        EditingLevelManager.Instance.GlobalSetting.IsInitialTrackLengthNotToEnd = isInitialTrackLengthNotToEndToggle.isOn;
    }

    public void OnInitialTrackLengthEndEdit()
    {
        if (int.TryParse(initialTrackLengthInputField.text, out int newLength))
        {
            if (newLength > 0)
            {
                initialTrackLength = newLength;
                EditingLevelManager.Instance.GlobalSetting.InitialTrackLength_Ms = newLength;
                return;
            }

        }
        initialTrackLengthInputField.text = initialTrackLength.ToString();
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
