using UnityEngine;

public class TurningPoint : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private GameObject point;
    [SerializeField] private Highlight2D highlight;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private EdgeCollider2D edgeCollider;
    [SerializeField] private LineRenderer lineRenderer;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value)
            {
                highlight.IsHighlight = true;
                lineRenderer.material.color = new Color(0.38f, 0.60f, 0.82f);
                TrackLineManager.Instance.AddSelectedItem(moveList, moveList[index]);
            }
            else
            {
                highlight.IsHighlight = false;
                lineRenderer.material.color = Color.white;
                TrackLineManager.Instance.RemoveSelectedItem(moveList, moveList[index]);
            }
            _isSelected = value;
        }
    }

    public (float time, float x, string curve) Item => moveList[index];

    // Private
    private bool IsStart => index == 0;
    private bool IsEnd => index == moveList.Count - 1;

    private BaseTrackMoveList moveList;
    private int index;
    private bool _isSelected = false;

    // Static
    private const string prefabPath = "Prefabs/EditorUI/TurningPoint";
    private static GameObject prefab = null;

    // Defined Functions
    public static TurningPoint DirectInstantiate(BaseTrackMoveList moveList, int index, Transform parent)
    {
        GetPrefab();
        GameObject instance = Instantiate(prefab);
        instance.transform.SetParent(parent, false);
        TurningPoint ret = instance.GetComponent<TurningPoint>();
        ret.Initialize(moveList, index);
        return ret;

        static void GetPrefab() { if (prefab == null) prefab = MyResources.Load<GameObject>(prefabPath); }
    }

    public void Initialize(BaseTrackMoveList moveList, int index)
    {
        this.moveList = moveList;
        this.index = index;
        var calcStart = new BaseNoteMoveList(moveList[index].time);
        calcStart.FixRaw(EditingLevelManager.Instance.MusicSetting.Speed);
        float startX = moveList[index].x;
        float startY = calcStart.GetPos(moveList[0].time).y;
        point.transform.localPosition = new(Camera.main.G2WPosX(startX), Camera.main.G2WPosY(startY));
        if (!IsEnd)
        {
            var calcEnd = new BaseNoteMoveList(moveList[index + 1].time);
            calcEnd.FixRaw(EditingLevelManager.Instance.MusicSetting.Speed);
            float endX = moveList[index + 1].x;
            float endY = calcEnd.GetPos(moveList[0].time).y;
            LineDrawer.DrawCurve(lineRenderer, new(Camera.main.G2WPosX(startX), Camera.main.G2WPosY(startY)), new(Camera.main.G2WPosX(endX), Camera.main.G2WPosY(endY)), moveList[index].curve, 0.15f);
            LineDrawer.DrawCurve(edgeCollider, new(Camera.main.G2WPosX(startX), Camera.main.G2WPosY(startY)), new(Camera.main.G2WPosX(endX), Camera.main.G2WPosY(endY)), moveList[index].curve, 0.1f);
        }
    }

    private string GetNextCurveLabel(string label)
    {
        return label switch
        {
            "u" => "s",
            "s" => "si",
            "si" => "so",
            "so" => "sb",
            "sb" => "sa",
            "sa" => "u",
            _ => "u"
        };
    }

    private void UpdateTime(float updatedTime)
    {
        try
        {
            if (IsStart)
            {
                CommandManager.Instance.Add(new UpdateTrackStartCommand(TrackLineManager.Instance.Track, updatedTime));
                IsSelected = true;
            }
            else if (IsEnd)
            {
                CommandManager.Instance.Add(new UpdateTrackEndCommand(TrackLineManager.Instance.Track, updatedTime));
                IsSelected = true;
            }
            else
            {
                var original = moveList[index];
                var updated = (updatedTime, original.x, original.curve);
                CommandManager.Instance.Add(new MoveListUpdateCommand(moveList, original, updated));
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败", HeaderMessage.MessageType.Warn);
        }
    }

    // System Functions
    void Awake()
    {
        EventManager.AddListener(EventManager.EventName.TurningPointUnselect, () => IsSelected = false);
    }

    void Update()
    {
        // TODO: 把这些都移到TrackLineManager中执行
        if (IsSelected)
        {
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToggleLineCurve))
            {
                if (!IsEnd)
                {
                    var original = moveList[index];
                    var updated = (original.time, original.x, GetNextCurveLabel(original.curve));
                    CommandManager.Instance.Add(new MoveListUpdateCommand(moveList, original, updated));
                }
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.Delete))
            {
                if (IsEnd || IsStart) HeaderMessage.Show("无法删除头尾结点", HeaderMessage.MessageType.Warn);
                else CommandManager.Instance.Add(new MoveListDeleteCommand(moveList, moveList[index]));
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToLeft))
            {
                var original = moveList[index];
                var updated = (original.time, original.x - 0.1f, original.curve);
                CommandManager.Instance.Add(new MoveListUpdateCommand(moveList, original, updated));
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToRight))
            {
                var original = moveList[index];
                var updated = (original.time, original.x + 0.1f, original.curve);
                CommandManager.Instance.Add(new MoveListUpdateCommand(moveList, original, updated));
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToLeftGrid))
            {
                if (GridManager.Instance.IsXGridShow)
                {
                    var original = moveList[index];
                    var updated = (original.time, GridManager.Instance.GetNearestXGridPos(original.x - 0.001f).left, original.curve);
                    CommandManager.Instance.Add(new MoveListUpdateCommand(moveList, original, updated));
                }
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToRightGrid))
            {
                if (GridManager.Instance.IsXGridShow)
                {
                    var original = moveList[index];
                    var updated = (original.time, GridManager.Instance.GetNearestXGridPos(original.x + 0.001f).right, original.curve);
                    CommandManager.Instance.Add(new MoveListUpdateCommand(moveList, original, updated));
                }
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToNext))
            {
                float updatedTime = moveList[index].time + 0.010f;
                UpdateTime(updatedTime);
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToPrevious))
            {
                float updatedTime = moveList[index].time - 0.010f;
                UpdateTime(updatedTime);
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToNextBeat))
            {
                if (GridManager.Instance.IsTGridShow)
                {
                    float updatedTime = GridManager.Instance.GetNearestTGridTime(moveList[index].time + 0.001f).ceiled;
                    UpdateTime(updatedTime);
                }
            }
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToPreviousBeat))
            {
                if (GridManager.Instance.IsTGridShow)
                {
                    float updatedTime = GridManager.Instance.GetNearestTGridTime(moveList[index].time - 0.001f).floored;
                    UpdateTime(updatedTime);
                }
            }
        }
    }
}
