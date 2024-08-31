using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JudgeUI : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] List<JudgeResult> judgeTypes = new(); // 只要使用到，就在Inspector面板上初始化它
    [SerializeField] TMP_Text countText;

    // Private
    private int judgeCount = 0;

    // Static

    // Defined Function
    void UpdateJudge(object judgeResult)
    {
        if (judgeTypes.Contains((JudgeResult)judgeResult))
        {
            judgeCount++;
            countText.text = judgeCount.ToString();
        }
    }

    void JudgeInit()
    {
        judgeCount = 0;
        countText.text = "0";
    }

    // System Function
    void Awake()
    {
        EventManager.AddListener(EventManager.EventName.UpdateJudge, UpdateJudge);
        EventManager.AddListener(EventManager.EventName.LevelInit, JudgeInit);
        JudgeInit();
    }
}
