using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditPanelManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private RectTransform test;

    [SerializeField] private TMP_Text selectTitle;
    [SerializeField] private RectTransform selectTrackList;
    [SerializeField] private RectTransform selectNoteList;
    [SerializeField] private RectTransform scrollContent;

    public static EditPanelManager Instance => _instance;

    // Private
    private bool shouldRender = false;

    // Static
    private static EditPanelManager _instance;

    // Defined Functions
    public void RenderTitle()
    {
        selectTitle.text = SelectManager.Instance.SelectTarget switch
        {
            SelectTarget.Track or SelectTarget.TurningPoint => $"选中{SelectManager.Instance.SelectedTracks.Count}条轨道",
            SelectTarget.Note => $"选中{SelectManager.Instance.SelectedNotes.Count}个Note",
            _ => "遇到了错误情况...",
        };
        RebuildLayout();
    }

    /// <summary> 重新构建编辑面板的内容 </summary>
    public void Render()
    {
        RenderTitle();
        switch (SelectManager.Instance.SelectTarget)
        {
            case SelectTarget.Track or SelectTarget.TurningPoint:
                selectNoteList.gameObject.SetActive(false);
                selectTrackList.gameObject.SetActive(true);
                for (int i = 0; i < selectTrackList.childCount; i++)
                {
                    Destroy(selectTrackList.GetChild(i).gameObject);
                }
                foreach (var track in SelectManager.Instance.SelectedTracks)
                {
                    EditTrackContent.DirectInstantiate(track, selectTrackList);
                }
                break;
            case SelectTarget.Note:
                selectNoteList.gameObject.SetActive(true);
                selectTrackList.gameObject.SetActive(false);
                for (int i = 0; i < selectNoteList.childCount; i++)
                {
                    Destroy(selectNoteList.GetChild(i).gameObject);
                }
                foreach (var note in SelectManager.Instance.SelectedNotes)
                {
                    EditNoteContent.DirectInstantiate(note, 1, selectNoteList);
                }
                break;
            default:
                Debug.LogError("SelectTarget error in SelectManager");
                break;
        }
        RebuildLayout();
        shouldRender = false;
    }

    /// <summary> 标记在Update中进行渲染，以避免重复请求的开销 </summary>
    public void AskForRender() => shouldRender = true;

    public void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectTrackList);
        LayoutRebuilder.ForceRebuildLayoutImmediate(selectNoteList);
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (shouldRender) Render();
    }
}
