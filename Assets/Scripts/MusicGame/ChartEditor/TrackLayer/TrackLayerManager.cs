using System.Collections.Generic;
using Takana3.MusicGame.LevelSelect;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrackLayerManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private TMP_Dropdown layerDropdown;
    [SerializeField] private Button addNewLayerButton;
    [SerializeField] private GameObject newLayerNameObject;
    [SerializeField] private TMP_InputField newLayerNameInputField;
    [SerializeField] private Button newLayerNameConfirmButton;
    [SerializeField] private Button newLayerNameCancelButton;

    public static TrackLayerManager Instance => _instance;

    public int SelectedLayer => layerDropdown.value;

    public LayerInfo LayerInfo { get; private set; }

    // Private

    // Static
    private static TrackLayerManager _instance;

    // Defined Functions
    /// <summary> ��ԭ�����ļ���ȡLayerInfo </summary>
    public void LoadLayer()
    {
        var levelInfo = InfoReader.ReadInfo<LevelInfo>();
        LayerInfo = levelInfo == null ? new() : levelInfo.LayerInfo;
    }

    /// <summary> ��EditingLevelManager������LayerInfo </summary>
    public void UpdateLayer()
    {
        LayerInfo.TrackBelongings.Clear();
        var info = EditingLevelManager.Instance.RawChartInfo;
        foreach (var component in info.ComponentList)
        {
            if (component is EditingTrack track)
            {
                LayerInfo.TrackBelongings.Add(track.Layer);
            }
        }
    }

    /// <summary> ���������ͬ���Ƶ�ͼ�������ʧ�� </summary>
    public bool AddLayer(string layerName)
    {
        if (layerName != string.Empty && !LayerInfo.LayerNames.Contains(layerName))
        {
            LayerInfo.LayerNames.Add(layerName);
            layerDropdown.options = GetOptions();
            layerDropdown.SetValueWithoutNotify(layerDropdown.options.Count - 1);
            EditPanelManager.Instance.Render();
            return true;
        }
        return false;
    }

    public List<TMP_Dropdown.OptionData> GetOptions()
    {
        List<TMP_Dropdown.OptionData> options = new()
        {
            new TMP_Dropdown.OptionData { text = "Ĭ��" },
            new TMP_Dropdown.OptionData { text = "װ��" },
            new TMP_Dropdown.OptionData { text = "����" }
        };
        foreach (var name in LayerInfo.LayerNames)
        {
            options.Add(new TMP_Dropdown.OptionData { text = name });
        }
        return options;
    }

    public void OnLayerValueChanged()
    {
        EventManager.Trigger(EventManager.EventName.ChangeTrackLayer, SelectedLayer);
    }

    public void OnAddNewLayerPressed()
    {
        newLayerNameObject.SetActive(true);
        newLayerNameInputField.text = $"Layer{LayerInfo.LayerNames.Count + 2}";
        InputManager.Instance.IsInputEnabled = false;
    }

    public void OnNewLayerNameConfirmPressed()
    {
        if (!AddLayer(newLayerNameInputField.text))
        {
            HeaderMessage.Show("���ͼ��ʧ�ܣ���������������", HeaderMessage.MessageType.Warn);
        }
        else
        {
            newLayerNameObject.SetActive(false);
            InputManager.Instance.IsInputEnabled = true;
        }
    }

    public void OnNewLayerNameCancelPressed()
    {
        newLayerNameObject.SetActive(false);
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void OnEnable()
    {
        LoadLayer();
        layerDropdown.interactable = true;
        layerDropdown.options = GetOptions();
        layerDropdown.SetValueWithoutNotify(0);
        EditingLevelManager.Instance.RawChartInfo.SetLayers(LayerInfo);
    }
}
