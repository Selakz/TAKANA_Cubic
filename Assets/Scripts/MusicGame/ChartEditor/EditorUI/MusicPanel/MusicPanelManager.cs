using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MusicPanelManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] TMP_Text musicVolumeText;
    [SerializeField] Slider effectVolumeSlider;
    [SerializeField] TMP_Text effectVolumeText;
    [SerializeField] RectTransform bpmListParent;

    public static MusicPanelManager Instance => _instance;

    // Private
    private List<(float time, float bpm)> bpmList = new() { (0f, 100f) };

    // Static
    private static MusicPanelManager _instance;

    // Defined Functions
    public void RenderBpmList()
    {
        bpmList.Sort((a, b) => a.time.CompareTo(b.time));
        for (int i = 0; i < bpmListParent.childCount; i++)
        {
            Destroy(bpmListParent.GetChild(i).gameObject);
        }
        BpmListItem.DirectInstantiate(bpmList[0], bpmListParent, 0);
        for (int i = 1; i < bpmList.Count; i++)
        {
            BpmListItem.DirectInstantiate(bpmList[i], bpmListParent, i);
        }
        GridManager.Instance.ResetGrids();
    }

    public void AddBpmListItem((float time, float bpm) item) => bpmList.Add(item);

    public void RemoveBpmListItem(int index) => bpmList.RemoveAt(index);

    public void OnMusicVolumeChange()
    {
        TimeProvider.Instance.Volume = musicVolumeSlider.value / 100f;
        musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value).ToString();
    }
    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        // 直接控制setting中的bpm列表
        musicVolumeSlider.interactable = true;
        bpmList = EditingLevelManager.Instance.SingleSetting.BpmList;
        RenderBpmList();
    }

    void Update()
    {

    }
}
