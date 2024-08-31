using Takana3.MusicGame.LevelSelect;
using Takana3.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.UI;

// 生成元件；接收并处理输入；暂停页面
public class LevelManager : MonoBehaviour
{
    // Serializable and Public
    public GameObject pauseMenu;
    public GameObject countdown;
    public JudgeLine defaultJudgeLine;
    public RawImage cover;

    // Private
    private float current;
    private ChartStatusManager chartStatusManager; // Though no method calls it, there are events calling it.
    private LevelInfo levelInfo;
    private Dictionary<JudgeType, NoteInputHandler> inputHandlers;
    private bool isPaused = true;
    private int componentIndex = 0;

    // Static
    private static LevelManager _instance = null;
    public static LevelManager Instance => _instance;

    // Defined Functions
    private void InstantiateComponents()
    {
        while (componentIndex < levelInfo.ChartInfo.ComponentList.Count
            && levelInfo.ChartInfo.ComponentList["Instantiate", componentIndex].TimeInstantiate < current)
        {
            Debug.Log($"Instantiate id {levelInfo.ChartInfo.ComponentList["Instantiate", componentIndex].Id} at time {current}");
            levelInfo.ChartInfo.ComponentList["Instantiate", componentIndex].Instantiate();
            componentIndex++;
        }
    }

    // 因为Input System没办法在输入的时候另外传参，所以只能拆成多个函数分别对应输入
    public void StoreInputT(InputAction.CallbackContext context)
    {
        if (context.interaction is PressInteraction)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    HandleInput(JudgeType.UnTrack);
                    break;
                default:
                    break;
            }
        }
    }
    public void StoreInputB(InputAction.CallbackContext context)
    {
        if (context.interaction is PressInteraction)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    HandleInput(JudgeType.Common);
                    break;
                default:
                    break;
            }
        }
    }
    public void StoreInputR(InputAction.CallbackContext context)
    {
        if (context.interaction is PressInteraction)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    HandleInput(JudgeType.Red);
                    break;
                default:
                    break;
            }
        }
    }
    public void StoreInputD(InputAction.CallbackContext context)
    {
        if (context.interaction is PressInteraction)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    HandleInput(JudgeType.Dark);
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleInput(JudgeType type)
    {
        if (!isPaused && levelInfo.MusicSetting.Mode == GameMode.Common)
        {
            inputHandlers[type]?.HandleInput(current);
        }
    }

    public void DetectPause(InputAction.CallbackContext context)
    {
        if (context.interaction is PressInteraction)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    if (isPaused == false && countdown.activeInHierarchy == false)
                    {
                        EventManager.Trigger(EventManager.EventName.Pause);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void Pause()
    {
        isPaused = true;
        pauseMenu.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
    }

    public void LevelInit()
    {
        componentIndex = 0;
        inputHandlers = new();
        foreach (var pair in levelInfo.ChartInfo.NoteDict)
        {
            inputHandlers.Add(pair.Key, new(pair.Value));
        }
        // 先把已有的InputInfo注入Note，之后在Note本身再作修改
        foreach (var info in levelInfo.InputInfos)
        {
            info.Note.InputInfo = info;
        }

        levelInfo.ChartInfo.Initialize(levelInfo.MusicSetting);
        chartStatusManager.Reset(levelInfo.ChartInfo, levelInfo.MusicSetting);
        isPaused = false;
        // new PackagedAnimator("BottomInfo", GameObject.Find("InfoCanvas").transform).Play();
    }

    IEnumerator FirstInit()
    {
        yield return new WaitForSeconds(1.0f);
        // new PackagedAnimator("FloatOver", GameObject.Find("UI").transform, false).Play();
    }

    // System Functions
    void Awake()
    {
        _instance = this;
        EventManager.AddListener(EventManager.EventName.LevelInit, LevelInit);
        EventManager.AddListener(EventManager.EventName.Pause, Pause);
        EventManager.AddListener(EventManager.EventName.Resume, Resume);

        levelInfo = InfoReader.ReadInfo<LevelInfo>() ?? new(new SongList().GetSongInfo(1), 1);
        chartStatusManager = new(levelInfo.ChartInfo, levelInfo.MusicSetting);
        cover.texture = levelInfo.Cover;
    }

    void Start()
    {
        StartCoroutine(FirstInit());
        EventManager.Trigger(EventManager.EventName.LevelInit);
    }

    void Update()
    {
        current = TimeProvider.Instance.ChartTime;
        InstantiateComponents();

        if (current > levelInfo.Music.length && levelInfo.ChartInfo.ComponentList["Instantiate", levelInfo.ChartInfo.ComponentList.Count - 1].ThisObject == null)
        {
            InfoReader.SetInfo(chartStatusManager.GetResultInfo(levelInfo));
            SceneLoader.LoadScene("ResultMenu", "Shutter", "SceneLoadDone");
        }
    }

    /// <summary>
    /// 辅助LevelManager输入处理的类。
    /// </summary>
    private sealed class NoteInputHandler
    {
        // Private
        private readonly MultiSortList<INote> notes;
        private int ptr = 0; // 全局指针

        // Defined Functions
        // TODO: 根据不同GameMode作不同处理
        public NoteInputHandler(MultiSortList<INote> notes)
        {
            Assert.IsTrue(notes.SortLabels.Contains("Judge"));
            this.notes = notes;
        }

        /// <summary>
        /// 找到距当前时间最近的note，并给它传输输入。
        /// </summary>
        public void HandleInput(float timeInput)
        {
            // TODO: 这样对于一种情况不太合理，考虑修改逻辑？
            if (ptr >= notes.Count) return;

            // 向后找到距输入时间最近的note
            float timeDiff = Math.Abs(notes["Judge", ptr].TimeJudge - timeInput);
            while (ptr < notes.Count - 1 && Math.Abs(notes["Judge", ptr + 1].TimeJudge - timeInput) < timeDiff)
            {
                ptr++;
                timeDiff = Math.Abs(notes["Judge", ptr].TimeJudge - timeInput);
            }
            // 传输输入，并观察是否有效
            if (ptr < notes.Count && notes["Judge", ptr].HandleInput(timeInput)) ptr++;
        }

        /// <summary>
        /// 重置指针位置
        /// </summary>
        public void Reset() { ptr = 0; }
    }
}
