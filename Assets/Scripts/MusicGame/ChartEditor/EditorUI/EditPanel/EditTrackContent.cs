using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditTrackContent : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_InputField timeStartInputField;
    [SerializeField] private TMP_InputField timeEndInputField;
    [SerializeField] private TMP_Dropdown layerDropdown;
    [SerializeField] private GameObject leftMoveListScroll;
    [SerializeField] private GameObject rightMoveListScroll;
    [SerializeField] private RectTransform leftMoveListItems;
    [SerializeField] private RectTransform rightMoveListItems;
    [SerializeField] private TMP_Text moveListTitle;
    [SerializeField] private TMP_Text moveListToggle;

    public EditingTrack Track { get; private set; }

    // Private
    private bool isLeftMoveListShow = true;
    private float start;
    private float end;
    private readonly List<(float time, float x, string curve)> rawLMoveList = new();
    private readonly List<(float time, float x, string curve)> rawRMoveList = new();

    // Static
    private const string prefabPath = "Prefabs/EditorUI/EditTrackContent";
    private static GameObject prefab = null;
    private static bool isFirstShowLeft = true;

    // Defined Functions
    public static EditTrackContent DirectInstantiate(EditingTrack track, RectTransform parent)
    {
        GetPrefab();
        GameObject instance = Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        EditTrackContent ret = instance.GetComponent<EditTrackContent>();
        ret.Initialize(track);
        return ret;

        static void GetPrefab() { if (prefab == null) prefab = MyResources.Load<GameObject>(prefabPath); }
    }

    public void Initialize(EditingTrack track)
    {
        Track = track;
        title.text = $"ID: {track.Id}";
        start = track.Track.TimeInstantiate;
        end = track.Track.TimeEnd;
        timeStartInputField.text = Mathf.RoundToInt(start * 1000).ToString();
        timeEndInputField.text = Mathf.RoundToInt(end * 1000).ToString();
        if (TrackLayerManager.Instance != null) layerDropdown.options = TrackLayerManager.Instance.GetOptions();
        layerDropdown.SetValueWithoutNotify(track.Layer);
        RenderMoveList();
        isLeftMoveListShow = isFirstShowLeft;
        DecideListShow();
    }

    // 只负责内容，不负责列表是否展开、选择了什么
    public void RenderMoveList()
    {
        // 左
        for (int i = 0; i < leftMoveListItems.childCount; i++)
        {
            Destroy(leftMoveListItems.GetChild(i).gameObject);
        }
        for (int i = 0; i < Track.Track.LMoveList.Count; i++)
        {
            EditMoveListItem.DirectInstantiate(Track.Track.LMoveList, i, leftMoveListItems);
        }
        // 右
        for (int i = 0; i < rightMoveListItems.childCount; i++)
        {
            Destroy(rightMoveListItems.GetChild(i).gameObject);
        }
        for (int i = 0; i < Track.Track.RMoveList.Count; i++)
        {
            EditMoveListItem.DirectInstantiate(Track.Track.RMoveList, i, rightMoveListItems);
        }
    }

    public void DecideListShow()
    {
        leftMoveListScroll.SetActive(isLeftMoveListShow);
        rightMoveListScroll.SetActive(!isLeftMoveListShow);
        moveListTitle.text = $"运动列表：{(isLeftMoveListShow ? "左" : "右")}边界";
    }

    public void ToggleMoveListShow()
    {
        isLeftMoveListShow = !isLeftMoveListShow;
        DecideListShow();
        isFirstShowLeft = isLeftMoveListShow;
    }

    public void OnUnselectPressed()
    {
        SelectManager.Instance.UnselectTrack(Track.Id);
        EditPanelManager.Instance.AskForRender();
    }

    public void OnDeletePressed()
    {
        CommandManager.Instance.Add(new DeleteTrackCommand(Track.Track));
        EditPanelManager.Instance.AskForRender();
    }

    public void OnStartEndEdit()
    {
        if (int.TryParse(timeStartInputField.text, out int newStartInt))
        {
            if (newStartInt == Mathf.RoundToInt(start * 1000f)) return;
            else
            {
                float newStart = newStartInt / 1000f;
                try
                {
                    var command = new UpdateTrackStartCommand(Track.Track, newStart);
                    CommandManager.Instance.Add(command);
                    return;
                }
                catch (Exception)
                {
                    HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
                    timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
                }
            }
        }
        else
        {
            HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
            timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
        }
    }

    public void OnEndEndEdit()
    {
        if (int.TryParse(timeEndInputField.text, out int newEndInt))
        {
            if (newEndInt == Mathf.RoundToInt(end * 1000f)) return;
            else
            {
                float newEnd = newEndInt / 1000f;
                try
                {
                    var command = new UpdateTrackEndCommand(Track.Track, newEnd);
                    CommandManager.Instance.Add(command);
                    return;
                }
                catch (Exception)
                {
                    HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
                    timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
                }
            }
        }
        else
        {
            HeaderMessage.Show("修改失败", HeaderMessage.MessageType.Warn);
            timeStartInputField.text = Mathf.RoundToInt(start * 1000f).ToString();
        }
    }

    public void OnLayerValueChanged()
    {
        if (layerDropdown.value == 1 && Track.Track.Notes.Count > 0)
        {
            HeaderMessage.Show("该轨道上存在Note，无法切换至装饰层", HeaderMessage.MessageType.Info);
            layerDropdown.SetValueWithoutNotify(Track.Layer);
            return;
        }
        Track.Layer = layerDropdown.value;
        if (TrackLayerManager.Instance != null) Track.OnSelectedLayerChanged(TrackLayerManager.Instance.SelectedLayer);
    }

    // System Functions
}
