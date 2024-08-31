using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Takana3.MusicGame.Values;

// ��������ٵĻ�����žͲ�̫����ô����
public class GridManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] Transform tGridParent;
    [SerializeField] Transform xGridParent;
    [SerializeField] TMP_InputField tGridInputField;
    [SerializeField] TMP_InputField xGridIntervalInputField;
    [SerializeField] TMP_InputField xGridOffsetInputField;
    [SerializeField] List<Selectable> selectables;

    public static GridManager Instance => _instance;
    public List<(float time, float bpm)> BpmList { get; set; } = new() { (0f, 60f), (86.571f, 175f) };
    public int LinesPerBeat { get; set; } = 4;
    public float XGridInterval { get; set; } = 1.5f;
    public float XGridOffset { get; set; } = 0f;

    public bool IsTGridShow { get => tGridParent.gameObject.activeSelf; set { tGridParent.gameObject.SetActive(value); if (value) ResetGrids(); } }
    public bool IsXGridShow { get => xGridParent.gameObject.activeSelf; set { xGridParent.gameObject.SetActive(value); } }

    // Private
    private ObjectPool<GameObject> tGridPool;
    private ObjectPool<GameObject> xGridPool;

    private float UpHeightLimitTime // ��ǰʱ����UpHeightLimit����ʱ��
        => GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed, UpHeightLimit);

    private float nextTGridTime = 0; // ��һ����δ������ĸ��߶�Ӧ��ʱ��
    private TGridType nextTGridType = TGridType.Default; // ��һ����δ������ĸ��߶�Ӧ������

    // Static
    private static GridManager _instance;
    private readonly static string tGridPrefabPath = "Prefabs/EditorUI/TGrid";
    private readonly static string xGridPrefabPath = "Prefabs/EditorUI/XGrid";

    // Defined Functions
    public void ResetGrids()
    {
        tGridPool.ReturnAll();
        xGridPool.ReturnAll();
        BpmList.Sort(((float time, float bpm) x, (float time, float bpm) y) => x.time.CompareTo(y.time));
        (_, nextTGridTime, _, nextTGridType) = GetNearestTGridTime(TimeProvider.Instance.ChartTime);

        GameObject xGrid = xGridPool.GetObject();
        xGrid.GetComponent<XGridController>().GamePos = XGridOffset;
        float left = XGridOffset - XGridInterval, right = XGridOffset + XGridInterval;
        while (left > -GameWidthSize)
        {
            xGrid = xGridPool.GetObject();
            xGrid.GetComponent<XGridController>().GamePos = left;
            left -= XGridInterval;
        }
        while (right < GameWidthSize)
        {
            xGrid = xGridPool.GetObject();
            xGrid.GetComponent<XGridController>().GamePos = right;
            right += XGridInterval;
        }
    }

    public Vector2 GetAttachedGamePoint(Vector2 gamePoint)
    {
        float time = GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed, gamePoint.y);
        var (floored, ceiled, _, _) = GetNearestTGridTime(time);
        time = Mathf.Abs(time - floored) < Mathf.Abs(time - ceiled) ? floored : ceiled;
        float y = GameTimeToY(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed, time);
        var (left, right) = GetNearestXGridPos(gamePoint.x);
        float x = Mathf.Abs(gamePoint.x - left) < Mathf.Abs(gamePoint.x - right) ? left : right;
        return new(x, y);
    }

    /// <summary> ʹĳ��TGrid����nextTGridTime�������¼���nextTGridTime��ֵ�����Ŀǰ����Ҫ���죬�����ͻ�pool </summary>
    public void AdoptTGrid(TGridController grid)
    {
        if (nextTGridTime > UpHeightLimitTime + 10.000f) tGridPool.ReturnObject(grid.gameObject);
        // ���칤��
        grid.Time = nextTGridTime;
        grid.Type = nextTGridType;
        // ���BpmList���ⲻ���Ϲ淶ʱ���Ҳ�
        if (BpmList.Count == 0) BpmList.Add((0, 175f));
        else if (BpmList.Count > 0 && BpmList[0].time != 0) BpmList.Insert(0, (0, 175));
        // ����nextTGridTime��Type
        (_, nextTGridTime, _, nextTGridType) = GetNearestTGridTime(nextTGridTime + 0.001f); // fxxk float
    }

    /// <summary> ��ȡĳһʱ�̵��������������������ʱ���Լ����ǵ����� </summary>
    public (float floored, float ceiled, TGridType flooredType, TGridType ceiledType) GetNearestTGridTime(float time)
    {
        if (time < 0) return (0, 0, TGridType.Beat, TGridType.Beat);
        // �ҵ������һ��С�ڸ���ʱ���bpm�ڵ�
        int index = 0;
        while (index + 1 < BpmList.Count && BpmList[index + 1].time < time) index++;
        // ������ӽ��ĸ���
        double timePerLine = 1 / (BpmList[index].bpm / 60) / LinesPerBeat;
        int lineCount = Mathf.FloorToInt((float)((time - BpmList[index].time) / timePerLine));
        double floored = BpmList[index].time + lineCount * timePerLine;
        double ceiled = floored + timePerLine;
        TGridType flooredType = GetType(LinesPerBeat, lineCount);
        TGridType ceiledType = GetType(LinesPerBeat, lineCount + 1);
        if (index + 1 < BpmList.Count && ceiled > BpmList[index + 1].time - 0.010f)
        {
            ceiled = BpmList[index + 1].time;
            ceiledType = TGridType.Beat;
        }
        return ((float)floored, (float)ceiled, flooredType, ceiledType);
    }

    public (float left, float right) GetNearestXGridPos(float x)
    {
        int lineCount = Mathf.FloorToInt((x - XGridOffset) / XGridInterval);
        float left = XGridOffset + lineCount * XGridInterval;
        float right = XGridOffset + (lineCount + 1) * XGridInterval;
        return (left, right);
    }

    private TGridType GetType(int linesPerBeat, int lineCount)
    {
        int remainder = lineCount % linesPerBeat;
        if (remainder == 0) return TGridType.Beat;
        if (linesPerBeat % 3 == 0)
        {
            int[] target = { 1, 2, 4, 5 };
            foreach (int i in target)
            {
                if (remainder == linesPerBeat * i / 6) return TGridType.Triplet;
            }
        }
        if (linesPerBeat % 2 == 0)
        {
            if (remainder == linesPerBeat / 2) return TGridType.Quaver;
            if (remainder == linesPerBeat / 4 || remainder == linesPerBeat * 3 / 4) return TGridType.SemiQuaver;
        }
        return TGridType.Default;
    }

    public void InputTGrid()
    {
        LinesPerBeat = int.Parse(tGridInputField.text);
        EditingLevelManager.Instance.SingleSetting.TGridLineCount = LinesPerBeat;
        ResetGrids();
    }

    public void InputXGridInterval()
    {
        XGridInterval = float.Parse(xGridIntervalInputField.text);
        EditingLevelManager.Instance.SingleSetting.XGridInterval = XGridInterval;
        ResetGrids();
    }

    public void InputXGridOffset()
    {
        XGridOffset = float.Parse(xGridOffsetInputField.text);
        EditingLevelManager.Instance.SingleSetting.XGridOffset = XGridOffset;
        ResetGrids();
    }

    // System Functions
    void Awake()
    {
        _instance = this;
        tGridPool = new(
            MyResources.Load<GameObject>(tGridPrefabPath),
            tGridParent,
            20,
            obj => obj.SetActive(false),
            obj => obj.SetActive(true),
            obj => obj.SetActive(false)
            );
        xGridPool = new(
            MyResources.Load<GameObject>(xGridPrefabPath),
            xGridParent,
            20,
            obj => obj.SetActive(false),
            obj => obj.SetActive(true),
            obj => obj.SetActive(false)
            );
    }

    void OnEnable()
    {
        BpmList = EditingLevelManager.Instance.SingleSetting.BpmList;
        LinesPerBeat = EditingLevelManager.Instance.SingleSetting.TGridLineCount;
        XGridInterval = EditingLevelManager.Instance.SingleSetting.XGridInterval;
        XGridOffset = EditingLevelManager.Instance.SingleSetting.XGridOffset;
        ResetGrids();
        tGridInputField.text = LinesPerBeat.ToString();
        xGridIntervalInputField.text = XGridInterval.ToString();
        xGridOffsetInputField.text = XGridOffset.ToString();
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
        // ���е�TGrid�����������ߣ�������һ��
        if (IsTGridShow)
        {
            while (nextTGridTime < UpHeightLimitTime)
            {
                GameObject gridObject = tGridPool.GetObject();
                AdoptTGrid(gridObject.GetComponent<TGridController>());
            }
        }
    }
}
