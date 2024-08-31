using Takana3.MusicGame.LevelSelect;
using Takana3.Settings;
using System;
using System.Collections.Generic;
using static Takana3.MusicGame.Values;

/// <summary>
/// 计算游玩过程中的各种谱面信息
/// </summary>
public class ChartStatusManager
{

    private List<InputInfo> inputInfos;
    private Dictionary<(NoteType, JudgeResult), int> judgeCount;

    private Dictionary<NoteType, double> noteScore; // float精度甚至真的不够用
    private double currentScore = 0f;
    private int currentCombo = 0;
    private int maxCombo = 0;

    public ChartStatusManager(ChartInfo chartInfo, MusicSetting setting)
    {
        Reset(chartInfo, setting);
        EventManager.AddListener(EventManager.EventName.AddJudgeAndInputInfo, AddJudgeAndInputInfo);
    }

    public void AddJudgeAndInputInfo(object jii)
    {
        // 因为Note最终得到的判定结果可能不止与InputInfo有关，所以需要另外传判定结果
        var (result, info) = ((JudgeResult, InputInfo))jii;
        inputInfos.Add(info);

        // Update Judge
        AddJudgeCount(info.Note.Type, result);
        EventManager.Trigger(EventManager.EventName.UpdateJudge, result);

        // Update Combo
        if (JudgeResult.LateMiss.BreakCombo().Contains(result)) currentCombo = 0;
        else currentCombo++;
        maxCombo = Math.Max(maxCombo, currentCombo);
        EventManager.Trigger(EventManager.EventName.UpdateCombo, currentCombo);

        // Update Score
        double score = noteScore[info.Note.Type] * info.Note.JudgeInfo[result];
        currentScore += score;
        EventManager.Trigger(EventManager.EventName.UpdateScore, currentScore);

        // TODO: Update Percentage
        // EventManager.Trigger(EventManager.EventName.UpdatePercentage, currentScore / maxScore);
    }

    public void Reset(ChartInfo chartInfo, MusicSetting setting)
    {
        judgeCount = new();
        inputInfos = new();
        noteScore = CalcScore(chartInfo);
        currentScore = 0f;
        currentCombo = 0;
        maxCombo = 0;
    }

    public static Dictionary<NoteType, double> CalcScore(ChartInfo chartInfo)
    {
        Dictionary<NoteType, double> noteScore = new();
        int comboSum = chartInfo.NoteSum + chartInfo[NoteType.Hold];
        double baseScore = MaxScore / comboSum;
        foreach (var type in NoteType.Tap.TNoteTypes())
        {
            noteScore.Add(type, baseScore);
        }
        return noteScore;
    }

    public ResultInfo GetResultInfo(LevelInfo levelInfo)
    {
        levelInfo.InputInfos = inputInfos;
        return new()
        {
            LevelInfo = levelInfo,
            JudgeCount = judgeCount,
            NoteScore = noteScore,
            Score = currentScore,
            MaxCombo = maxCombo,
        };
    }

    private void AddJudgeCount(NoteType noteType, JudgeResult result)
    {
        if (judgeCount.ContainsKey((noteType, result))) judgeCount[(noteType, result)]++;
        else judgeCount.Add((noteType, result), 1);
    }
}
