using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 规定在屏幕中将父物体所在的位置与判定线对齐
public class TGridController : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private float lineWidth;
    [SerializeField] private float blockWidth;
    [SerializeField] private float height;
    [SerializeField] private float pixelPerGameY;
    [SerializeField] private float pixelPerGameYForTimingBlock;
    [SerializeField] private RectTransform self;
    [SerializeField] private RectTransform beatLine;
    [SerializeField] private Image lineImage;
    [SerializeField] private TimingBlock timingBlock;
    [SerializeField] private RectTransform timingBlockTransform;
    [SerializeField] private RectTransform timingBlockParent;

    public float Time
    {
        get => time;
        set
        {
            time = value;
            moveList = new(time);
            moveList.FixRaw(EditingLevelManager.Instance.MusicSetting.Speed);
        }
    }
    public TGridType Type
    {
        get => type;
        set
        {
            type = value;
            lineImage.color = value.GetColor();
            timingBlock.image.color = value.GetColor();
        }
    }

    public float GamePos
    {
        get => self.anchoredPosition.y / pixelPerGameY;
        set
        {
            self.anchoredPosition = new(0, value * pixelPerGameY);
            timingBlockTransform.anchoredPosition = new(0, value * pixelPerGameYForTimingBlock);
        }
    }

    // Private
    private float time;
    private TGridType type;
    private BaseNoteMoveList moveList;

    // Static

    // Defined Functions
    public void UpdatePos()
    {
        GamePos = moveList.GetPos(TimeProvider.Instance.ChartTime).y;
    }

    // System Functions
    void Start()
    {
        timingBlockParent = (RectTransform)GameObject.Find("/EditorCanvas/TimingBlocks").transform;
        timingBlockTransform.SetParent(timingBlockParent, false);
    }

    void Update()
    {
        UpdatePos();
        if (Time < TimeProvider.Instance.ChartTime)
        {
            GridManager.Instance.AdoptTGrid(this);
        }
    }

    void OnDisable()
    {
        if (timingBlock != null) timingBlock.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (timingBlock != null) timingBlock.gameObject.SetActive(true);
    }
}
