using System;
using System.Collections.Generic;
using System.IO;
using Takana3.MusicGame.LevelSelect;
using Takana3.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// TODO: 将设置相关的内容分离出一个新的Manager
// 编辑器界面的LevelManager
public class EditingLevelManager : MonoBehaviour
{
    // Serializable and Public
    public JudgeLine defaultJudgeLine;
    public RawImage cover;
    [SerializeField] private List<MonoBehaviour> CanAndToEnables; // TODO: 分散到各个Manager中，并善用Selectable

    public static EditingLevelManager Instance => _instance;
    public string LevelPath { get; private set; }
    public SongInfo SongInfo { get; private set; }
    public RawChartInfo RawChartInfo { get; private set; }
    public MusicSetting MusicSetting { get; private set; }
    public EditorGlobalSetting GlobalSetting { get; private set; }
    public EditorSingleSetting SingleSetting { get; private set; }

    // Private
    private float Current => TimeProvider.Instance.ChartTime;
    private ChartStatusManager chartStatusManager;
    private LevelInfo levelInfo;
    private bool isPaused = true;
    private int componentIndex = 0;
    private float shouldResetFieldTo = -1f; // 当值大于等于0时，表示应重置，且重置时间即为该值
    private bool shouldResetField = false;
    private float timer = 0f;

    // Static
    private static EditingLevelManager _instance = null;

    // Defined Functions
    private void InstantiateComponents()
    {
        while (componentIndex < RawChartInfo.ComponentList.Count
            && RawChartInfo.ComponentList["Instantiate", componentIndex].TimeInstantiate < Current)
        {
            var item = RawChartInfo.ComponentList["Instantiate", componentIndex];
            item.Instantiate();
            componentIndex++;
        }
    }

    public void TogglePause()
    {
        if (isPaused) EventManager.Trigger(EventManager.EventName.Resume);
        else EventManager.Trigger(EventManager.EventName.Pause);
    }

    public void Pause() => isPaused = true;

    public void Resume() => isPaused = false;

    public void LevelInit()
    {
        componentIndex = 0;
        RawChartInfo.Initialize(MusicSetting);
        defaultJudgeLine = RawChartInfo.ComponentList[0].Component as JudgeLine;
        chartStatusManager.Reset(RawChartInfo.ToChartInfo(), MusicSetting);

        isPaused = true;
    }

    public void ResetFieldTo(float time)
    {
        var isPrePaused = isPaused;
        EventManager.Trigger(EventManager.EventName.LevelInit);
        TimeProvider.Instance.ChartTime = time;
        if (GridManager.Instance != null) GridManager.Instance.ResetGrids();
        if (TrackLineManager.Instance != null) TrackLineManager.Instance.Decorate(TrackLineManager.Instance.Track);
        if (!isPrePaused) EventManager.Trigger(EventManager.EventName.Resume);
        shouldResetFieldTo = -1f;
    }

    public void ResetField() { ResetFieldTo(Current); shouldResetField = false; }

    public void AskForResetFieldTo(float time) => shouldResetFieldTo = time;

    public void AskForResetField() => shouldResetField = true;

    public void SaveProject()
    {
        LayerInfo layerInfo = null;
        if (TrackLayerManager.Instance != null)
        {
            TrackLayerManager.Instance.UpdateLayer();
            layerInfo = TrackLayerManager.Instance.LayerInfo;
        }
        File.WriteAllText(Path.Combine(LevelPath, $"{SingleSetting.Difficulty}.dlf"), RawChartInfo.ToChartInfo().ToSimpleDLF(layerInfo));
        MusicSetting.EditorSave();
        GlobalSetting.Save();
        SingleSetting.Save(Path.Combine(LevelPath, "Takana_Data", "setting.json"));
        SongInfo.Save(Path.Combine(LevelPath, "Takana_Data", "song.json"));
        HeaderMessage.Show("保存成功！", HeaderMessage.MessageType.Success);
    }

    public void AutoSaveChart()
    {
        File.WriteAllText(Path.Combine(LevelPath, "Takana_Data", "AutoSave", $"{SingleSetting.Difficulty}_{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.dlf"), RawChartInfo.ToChartInfo().ToSimpleDLF());
        Debug.Log("自动保存谱面");
    }

    // System Functions
    void Awake()
    {
        _instance = this;
        EventManager.AddListener(EventManager.EventName.LevelInit, LevelInit);
        EventManager.AddListener(EventManager.EventName.Pause, Pause);
        EventManager.AddListener(EventManager.EventName.Resume, Resume);
    }

    void OnEnable()
    {
        timer = Time.time;
        levelInfo = InfoReader.ReadInfo<LevelInfo>();
        RawChartInfo = levelInfo.ChartInfo.ToRawChartInfo();
        MusicSetting = levelInfo.MusicSetting;
        LevelPath = levelInfo.LevelPath;
        SongInfo = levelInfo.SongInfo;
        GlobalSetting = EditorGlobalSetting.Load();
        GlobalSetting.LastOpenedPath = LevelPath;
        SingleSetting = EditorSingleSetting.Load(Path.Combine(levelInfo.LevelPath, "Takana_Data", "setting.json"));
        chartStatusManager = new(levelInfo.ChartInfo, MusicSetting);
        cover.texture = levelInfo.Cover;
        foreach (var toEnable in CanAndToEnables)
        {
            if (toEnable is ICanEnableUI ui) ui.Enable();
        }
        EventManager.Trigger(EventManager.EventName.LevelInit);
    }

    void OnDisable()
    {
        // 以一种只能说可行的方式清空了原谱面的所有游戏物体
        if (defaultJudgeLine.ThisObject != null) Destroy(defaultJudgeLine.ThisObject);
    }

    void Update()
    {
        if (shouldResetField) ResetField();
        if (shouldResetFieldTo > 0) ResetFieldTo(shouldResetFieldTo);

        if (GlobalSetting.IsAutoSaveAllowed && GlobalSetting.AutoSaveInterval_Minute > 0)
        {
            if (Time.time - timer >= GlobalSetting.AutoSaveInterval_Minute * 60f)
            {
                AutoSaveChart();
                timer = Time.time;
            }
        }
        if (GlobalSetting.IsForcePauseAllowed)
        {
            if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.EditorBasic.ForcePause))
            {
                if (!isPaused && Camera.main.ContainsScreenPoint(Input.mousePosition))
                {

                    EventManager.Trigger(EventManager.EventName.Pause);
                }
            }
        }

        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.EditorBasic.Pause)) TogglePause();
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.EditorBasic.Save)) SaveProject();

        // 鼠标滚轮控制歌曲进度
        float y = Mouse.current.scroll.ReadValue().y;
        if (y != 0 && Camera.main.ContainsScreenPoint(Input.mousePosition))
        {
            if (GridManager.Instance != null && GridManager.Instance.IsTGridShow)
            {
                if (y > 0) ResetFieldTo(GridManager.Instance.GetNearestTGridTime(Current + 0.001f).ceiled);
                if (y < 0) ResetFieldTo(GridManager.Instance.GetNearestTGridTime(Current - 0.001f).floored);
            }
            else
            {
                ResetFieldTo(Current + (y > 0 ? 0.5f : -0.5f));
            }
        }
    }

    void LateUpdate()
    {
        if (!shouldResetField && shouldResetFieldTo < 0) InstantiateComponents();
    }
}
