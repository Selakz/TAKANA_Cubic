using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayfieldUI : MonoBehaviour
{
    // TODO: �����е�PlayfieldUI����Ǩ�Ƶ�����ļ�?
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
