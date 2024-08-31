using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfieldUI : MonoBehaviour
{
    // TODO: 将所有的PlayfieldUI工作迁移到这个文件?
    // Judge Indicator
    public GameObject judgeIndicator;
    public Animator judgeIndicatorAnimator;

    // Defined Functions
    public void JudgeIndicatorPop(object judgeResult)
    {
        if (!judgeIndicator.activeSelf) judgeIndicator.SetActive(true);
        judgeIndicatorAnimator.Play(0);
    }

    // System Function
    void Start()
    {
        EventManager.AddListener(EventManager.EventName.UpdateJudge, JudgeIndicatorPop);
    }
}
