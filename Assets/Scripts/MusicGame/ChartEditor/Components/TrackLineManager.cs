using UnityEngine;
using static Takana3.MusicGame.Values;

public class TrackLineManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] Transform indicator;
    [SerializeField] Transform pointsParent;

    public static TrackLineManager Instance => _instance;
    public Track Track { get; private set; }

    public float GamePos
    {
        get => Camera.main.W2GPosX(transform.localPosition.x);
        private set
        {
            transform.localPosition = new(0, Camera.main.G2WPosY(value));
        }
    }

    // 以下两个属性，只是用于在重新渲染的时候决定要选中哪个点
    public BaseTrackMoveList SelectedMoveList { get; set; }

    public (float time, float x, string curve) SelectedItem { get; set; }

    // Private
    private float Current => TimeProvider.Instance.ChartTime;

    private BaseNoteMoveList moveList;

    // Static
    private static TrackLineManager _instance;

    // Defined Functions
    public void Decorate(Track track)
    {
        Track = track;
        if (track == null) return;
        moveList = new(track.TimeInstantiate);
        moveList.FixRaw(EditingLevelManager.Instance.MusicSetting.Speed);
        SpriteInit();
        for (int i = 0; i < pointsParent.childCount; i++)
        {
            Destroy(pointsParent.GetChild(i).gameObject);
        }
        for (int i = 0; i < track.LMoveList.Count; i++)
        {
            var point = TurningPoint.DirectInstantiate(track.LMoveList, i, pointsParent);
            if (track.LMoveList == SelectedMoveList && track.LMoveList[i] == SelectedItem) point.Select();
        }
        for (int i = 0; i < track.RMoveList.Count; i++)
        {
            var point = TurningPoint.DirectInstantiate(track.RMoveList, i, pointsParent);
            if (track.RMoveList == SelectedMoveList && track.RMoveList[i] == SelectedItem) point.Select();
        }
    }

    public void Clear()
    {
        indicator.gameObject.SetActive(false);
        for (int i = 0; i < pointsParent.childCount; i++)
        {
            Destroy(pointsParent.GetChild(i).gameObject);
        }
        Track = null;
        SelectedMoveList = null;
    }

    private void SpriteInit()
    {
        float left = Track.GetX(Track.TimeInstantiate, true), right = Track.GetX(Track.TimeInstantiate, false);
        indicator.gameObject.SetActive(true);
        indicator.localPosition = new(Camera.main.G2WPosX((left + right) / 2), indicator.localPosition.y);
        indicator.localScale = new(Camera.main.G2WPosX(Mathf.Abs(left - right)), Camera.main.G2WPosY(0.3f));
    }

    public void UpdatePos()
    {
        GamePos = moveList.GetPos(Current).y;
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        if (Track != null)
        {
            UpdatePos();
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.CreateTurningPoint))
            {
                var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
                float baseX = gamePoint.x;
                if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
                float baseTime = GameYToTime(Current, EditingLevelManager.Instance.MusicSetting.Speed, gamePoint.y);
                if (baseTime < Track.TimeInstantiate || baseTime > Track.TimeEnd)
                {
                    HeaderMessage.Show("添加失败：目标时间在轨道时间范围外", HeaderMessage.MessageType.Warn);
                    return;
                }
                float leftX = Track.GetX(baseTime, true), rightX = Track.GetX(baseTime, false);
                bool isLeft = baseX < (leftX + rightX) / 2;
                (float time, float x, string curve) newItem = (baseTime, GridManager.Instance.IsXGridShow ? gamePoint.x : baseX, "u");
                CommandManager.Instance.Add(new MoveListInsertCommand(isLeft ? Track.LMoveList : Track.RMoveList, newItem));
            }
        }
    }
}
