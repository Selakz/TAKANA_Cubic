using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BpmListItem : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private TMP_InputField timingInputField;
    [SerializeField] private TMP_InputField bpmInputField;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button addButton;

    public (float time, float bpm) Item => (time, bpm);

    public int Index { get; private set; }

    // Private
    private float time = 0f;
    private float bpm = 100f;

    // Static
    private const string prefabPath = "Prefabs/EditorUI/BpmListItem";
    private static GameObject prefab = null;

    // Defined Functions
    public static BpmListItem DirectInstantiate((float time, float bpm) item, RectTransform parent, int index)
    {
        GetPrefab();
        GameObject instance = Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        BpmListItem ret = instance.GetComponent<BpmListItem>();
        ret.Index = index;
        ret.Initialize(item);
        if (index == 0)
        {
            ret.timingInputField.interactable = false;
            ret.deleteButton.interactable = false;
        }
        return ret;

        static void GetPrefab() { if (prefab == null) prefab = MyResources.Load<GameObject>(prefabPath); }
    }

    public void Initialize((float time, float bpm) item)
    {
        (time, bpm) = item;
        timingInputField.text = Mathf.RoundToInt(time * 1000f).ToString();
        bpmInputField.text = bpm.ToString("0.000");
    }

    public void OnAddPressed() { MusicPanelManager.Instance.AddBpmListItem(Item); MusicPanelManager.Instance.RenderBpmList(); }

    public void OnDeletePressed() { MusicPanelManager.Instance.RemoveBpmListItem(Index); MusicPanelManager.Instance.RenderBpmList(); }

    public void OnTimingEndEdit()
    {
        if (int.TryParse(timingInputField.text, out int newTiming))
        {
            if (newTiming >= 0)
            {
                time = newTiming / 1000f;
                MusicPanelManager.Instance.AddBpmListItem(Item);
                MusicPanelManager.Instance.RemoveBpmListItem(Index);
                MusicPanelManager.Instance.RenderBpmList();
                return;
            }
        }
        timingInputField.text = Mathf.RoundToInt(time * 1000f).ToString();
    }

    public void OnBpmEndEdit()
    {
        if (float.TryParse(bpmInputField.text, out float newBpm))
        {
            if (newBpm > 0)
            {
                bpm = newBpm;
                MusicPanelManager.Instance.AddBpmListItem(Item);
                MusicPanelManager.Instance.RemoveBpmListItem(Index);
                MusicPanelManager.Instance.RenderBpmList();
                return;
            }
        }
        bpmInputField.text = bpm.ToString("0.000");
    }

    // System Functions
}
