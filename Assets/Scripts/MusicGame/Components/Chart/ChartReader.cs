using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Takana3.Settings;
using UnityEngine;
using static Takana3.MusicGame.Values;

public class ChartReader
{
    // 用于初始化ChartInfo
    private float offset = 0f;
    private readonly Dictionary<JudgeType, List<INote>> noteList = new();
    private readonly List<IComponent> componentList = new();
    private readonly Dictionary<string, string> customInfo = new();

    // 读谱过程的全局变量
    private string[] chart;
    private int ptr = 0; // 谱面行数指针
    private int g_ID = 0; // 通过自增为每个元件赋予ID
    private bool isOffsetDone = false;
    private bool isLayerNameDone = false;
    private bool isLayerOrderDone = false;
    private bool isChartProcess = false;
    private Tap g_Tap; // 全局press指针
    private Slide g_Slide; // 全局press指针
    private Hold g_Hold; // 全局thold指针
    private Track g_Track; // 全局track指针
    private JudgeLine g_Line; // 全局judgeline指针
    private LayerInfo layerInfo = new(); // 编辑器下产出图层信息
    private Dictionary<int, int> layerLiteralToActual = new();

    // Static
    // 看来有些表达式之后不太能写成常量的样子...
    private const string uInt = @"\d+", Int = @"-?\d+", uFloat = @"\d+(?:\.\d+)?", Float = @"-?\d+(?:\.\d+)?", Bool = @"(?:True|true|False|false)";
    private const string Curve = @"(?:\S{1,2})";
    //private const string Color = @"(?:t|r|b|d)";
    private enum Regexes
    {
        Empty,
        Offset, Split, Custom,
        UTap, UTapS, UPos, Speed, Trail,
        Track, LPos, RPos,
        Tap, TapS,
        Slide, SlideS,
        Hold, HoldS, Scale,
        Camera, Plot,
        LayerName, LayerOrder
    }

    // TODO: 整理正则表达式信息（不是哥们我之前怎么就没有用扩展方法呢我请问了？？？）
    private readonly Dictionary<Regexes, string> regexes = new(){
        {Regexes.Empty, @"^\s*$"},
        {Regexes.Offset, @$"^\s*offset\s*\(\s*({uInt})\s*\);"},
        {Regexes.Split, @"^\s*-+\s*$"},
        {Regexes.Custom, @"^\s*(\S*)\s*:\s*(\S*)\s*;"},
        {Regexes.UTap, @$"^\s*upress\s*\(\s*({uInt}),\s*({uFloat}),\s*({Float})\s*\)\s*;"},
        {Regexes.UTapS, @$"^\s*upress\s*\(\s*({uInt}),\s*({uFloat}),\s*({Float}),\s*({Float})\s*\)\s*;"},
        {Regexes.UPos, @$"^\s*pos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"},
        {Regexes.Speed, @$"^\s*speed\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;"},
        {Regexes.Trail, @$"^\s*trail\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;"},

        // 理论上原游戏只需要用到以下的表达式
        {Regexes.Track, @$"^\s*track\s*\(\s*({uInt}),\s*({uInt}),\s*({Float}),\s*({Float})\s*\)\s*;"},
        {Regexes.LPos, @$"^\s*lpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"},
        {Regexes.RPos, @$"^\s*rpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"},
        {Regexes.Tap, @$"^\s*tap\s*\(\s*({uInt})\s*\)\s*;"},
        {Regexes.TapS, @$"^\s*tap\s*\(\s*({uInt}),\s*({Float})\s*\)\s*;"},
        {Regexes.Slide, @$"^\s*slide\s*\(\s*({uInt})\s*\)\s*;"},
        {Regexes.SlideS, @$"^\s*slide\s*\(\s*({uInt}),\s*({Float})\s*\)\s*;"},
        {Regexes.Hold, @$"^\s*hold\s*\(\s*({uInt}),\s*({uInt})\s*\)\s*;"},
        {Regexes.HoldS, @$"^\s*hold\s*\(\s*({uInt}),\s*({uInt}),\s*({Float})\s*\)\s*;"},
        {Regexes.LayerName, @$"^\s*layername\s*\(((?:\s*{uInt},\s*\S*,)*(?:\s*{uInt},\s*\S*))\s*\)\s*;" },
        {Regexes.LayerOrder, @$"^\s*layerorder\s*\(((?:\s*{uInt},)*(?:\s*{uInt}))\s*\)\s*;" },

        {Regexes.Scale, @$"^\s*scale\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;"},
        {Regexes.Camera, @$"camera"},
        {Regexes.Plot, @$"plot"},
    };

    private readonly HashSet<Regexes> BaseRegex = new(){
        Regexes.Empty, Regexes.Offset, Regexes.Split, Regexes.Custom, Regexes.LayerName, Regexes.LayerOrder,
        Regexes.UTap, Regexes.UTapS, Regexes.Track, Regexes.Camera, Regexes.Plot,
    };

    private readonly HashSet<Regexes> UNoteRegex = new(){
            Regexes.UPos, Regexes.Speed, Regexes.Trail, Regexes.Empty,
    };

    private readonly HashSet<Regexes> TrackRegex = new(){
            Regexes.LPos, Regexes.Tap, Regexes.TapS, Regexes.Slide, Regexes.SlideS, Regexes.Hold, Regexes.HoldS, Regexes.Empty
    };

    private readonly HashSet<Regexes> TNoteRegex = new(){
            Regexes.Speed, Regexes.Trail, Regexes.Empty
    };

    private readonly List<Regexes> THoldRegex = new(){
            Regexes.Speed, Regexes.Trail, Regexes.Scale, Regexes.Empty
    };

    // Defined Function
    private ChartReader() { }

    public static ChartInfo Read(string songId, int difficulty, MusicSetting setting)
    {
        var rawChart = MyResources.Load<TextAsset>($"Levels/{songId}/{difficulty}").text;
        ChartInfo chartInfo = new ChartReader().InClassRead(rawChart);
        chartInfo.Initialize(setting);
        return chartInfo;
    }

    public static ChartInfo ReadUninitialized(string songId, int difficulty)
    {
        var rawChart = MyResources.Load<TextAsset>($"Levels/{songId}/{difficulty}").text;
        ChartInfo chartInfo = new ChartReader().InClassRead(rawChart);
        return chartInfo;
    }

    public static ChartInfo ReadExternal(string path, int difficulty, MusicSetting setting, out LayerInfo layerInfo)
    {
        var rawChart = File.ReadAllText($"{path}/{difficulty}.dlf");
        ChartReader chartReader = new();
        ChartInfo chartInfo = chartReader.InClassRead(rawChart);
        chartInfo.Initialize(setting);
        layerInfo = chartReader.layerInfo;
        return chartInfo;
    }

    public static ChartInfo ReadExternalUninitialized(string path, int difficulty)
    {
        var rawChart = File.ReadAllText($"{path}/{difficulty}.dlf");
        ChartInfo chartInfo = new ChartReader().InClassRead(rawChart);
        return chartInfo;
    }

    private ChartInfo InClassRead(string rawChart)
    {
        chart = rawChart.Split('\n');
        DeleteAnnotation(chart);

        foreach (JudgeType type in Enum.GetValues(typeof(JudgeType)))
        {
            noteList[type] = new();
        }

        g_Line = new(g_ID++); // 默认的初始判定线。以后可能会修改逻辑
        componentList.Add(g_Line);

        for (ptr = 0; ptr < chart.Length; ptr++)
        {
            bool isValidLine = false;
            foreach (var pair in regexes)
            {
                var match = Regex.Match(chart[ptr], pair.Value);
                if (match.Success && BaseRegex.Contains(pair.Key))
                {
                    Action<Match> action;
                    if (!isChartProcess)
                    {
                        action = pair.Key switch
                        {
                            Regexes.Empty => CaseDoNothing,
                            Regexes.Offset => CaseOffset,
                            Regexes.Split => CaseSplit,
                            Regexes.Custom => CaseCustom,
                            Regexes.LayerName => CaseLayerName,
                            Regexes.LayerOrder => CaseLayerOrder,
                            _ => AssertError
                        };
                    }
                    else
                    {
                        action = pair.Key switch
                        {
                            Regexes.Empty => CaseDoNothing,
                            Regexes.UTap => CaseUTap,
                            Regexes.UTapS => CaseUTap,
                            Regexes.Track => CaseTrack,
                            Regexes.Camera => CaseCamera,
                            Regexes.Plot => CasePlot,
                            _ => AssertError
                        };
                    }
                    action.Invoke(match);
                    isValidLine = true;
                    break;
                }
            }
            if (!isValidLine) AssertError("Find invalid content.");
        }

        return new(offset, noteList, new(componentList), customInfo);
    }

    public static void DeleteAnnotation(string[] chart)
    {
        for (int i = 0; i < chart.Length; i++)
        {
            int annoPos = chart[i].IndexOf("//");
            if (annoPos >= 0) chart[i] = chart[i].Remove(annoPos);
        }
    }

    private void CaseDoNothing(Match match) { }

    private void CaseOffset(Match match)
    {
        // @$"^\s*offset\s*\(\s*({uInt})\s*\);"
        if (!isOffsetDone && !isChartProcess)
        {
            offset = int.Parse(match.Groups[1].Value) / 1000f;
            isOffsetDone = true;
        }
        else
        {
            AssertError("Wrong offset position at line: ");
        }
    }

    private void CaseSplit(Match match)
    {
        // @"^\s*-+\s*$"
        if (!isChartProcess)
        {
            // 虽然传入参数没什么用但是为了方便委托还是加了...
            isChartProcess = true;
        }
        else
        {
            AssertError("Duplicate split line in chart.");
        }
    }

    private void CaseCustom(Match match)
    {
        // @"^\s*(\S*)\s*:\s*(\S*)\s*;"
        if (!isChartProcess)
        {
            customInfo.Add(match.Groups[1].Value, match.Groups[2].Value);
        }
        else
        {
            AssertError("Wrong custom information position at line: ");
        }
    }

    private void CaseUTap(Match match)
    {
        float timeJudge = int.Parse(match.Groups[1].Value) / 1000f;
        float width = float.Parse(match.Groups[2].Value);
        float posEnd = float.Parse(match.Groups[3].Value);
        float speedRate = match.Groups.Count < 5 ? 1 : float.Parse(match.Groups[4].Value);

        // 构造一个完整的UPress和一个完整的Track
        g_Track = new(g_ID++, TrackType.Transparent, -TimePreAnimation, timeJudge, posEnd - width / 2, posEnd + width / 2, false, false, false, g_Line);
        g_Tap = new(g_ID++, NoteType.UTap, timeJudge, g_Track, speedRate);
        // 将track的生成时间设为最低速下Note的生成时间
        g_Track.TimeInstantiate = g_Tap.CalcTimeInstantiate(MinSpeed, false);
        noteList[g_Tap.JudgeType].Add(g_Tap);
        componentList.Add(g_Tap);
        componentList.Add(g_Track);

        // 处理unote的所有修饰行
        bool isNewElement = false, hasPosLine = false, hasSpeedLine = false;
        while (!isNewElement && ptr + 1 < chart.Length)
        {
            bool isValidLine = false;
            foreach (var pair in regexes)
            {
                var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
                if (nextMatch.Success)
                {
                    isValidLine = true;
                    if (UNoteRegex.Contains(pair.Key))
                    {
                        ptr++;
                        switch (pair.Key)
                        {
                            case Regexes.UPos:
                                if (!hasPosLine)
                                {
                                    hasPosLine = true;
                                    CaseUPos(nextMatch, width, posEnd);
                                }
                                else AssertError("Duplicate position property.");
                                break;
                            case Regexes.Speed:
                                if (!hasSpeedLine)
                                {
                                    hasSpeedLine = true;
                                    CaseSpeed(nextMatch);
                                }
                                else AssertError("Duplicate movement property.");
                                break;
                            case Regexes.Trail:
                                if (!hasSpeedLine)
                                {
                                    hasSpeedLine = true;
                                    CaseTrail(nextMatch);
                                }
                                else AssertError("Duplicate movement property.");
                                break;
                        }
                        break;
                    }
                    else if (BaseRegex.Contains(pair.Key))
                    {
                        isValidLine = true;
                        isNewElement = true;
                        break;
                    }
                    else AssertError("Property appears in a wrong place.");
                }
            }
            if (!isValidLine)
            {
                ptr++;
                AssertError("Find invalid content.");
            }
        }
    }

    private void CaseUPos(Match match, float width, float posEnd)
    {
        // @$"^\s*pos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
        string[] posList = match.Groups[1].Value.Split(',');
        List<(float time, float x, string curve)> lPosList = new(), rPosList = new();

        for (int i = 0; i < posList.Length; i += 3)
        {
            // (time, pos, curve)
            float time = int.Parse(posList[i]) / 1000f;
            float pos = float.Parse(posList[i + 1]);
            string curve = posList[i + 2].Trim();
            if (lPosList.Count != 0 && time < lPosList[^1].time) AssertError("The time in pos line is not in nondecreasing order.");
            if (time < g_Track.TimeInstantiate || time > g_Track.TimeEnd) AssertError("The time in pos line is out of range.");
            lPosList.Add((time, pos - width / 2, curve));
            rPosList.Add((time, pos + width / 2, curve));
        }
        g_Track.LMoveList = new(lPosList, g_Track.TimeInstantiate, g_Track.TimeEnd, posEnd - width / 2);
        g_Track.RMoveList = new(rPosList, g_Track.TimeInstantiate, g_Track.TimeEnd, posEnd + width / 2);
    }

    private void CaseSpeed(Match match)
    {
        // @$"^\s*speed\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
        string[] speedLine = match.Groups[1].Value.Split(',');

        List<(float time, float speedRate)> speedList = new();
        for (int i = 0; i < speedLine.Length; i += 2)
        {
            // (time, speedRate)
            float time = int.Parse(speedLine[i]) / 1000f;
            float speedRate = float.Parse(speedLine[i + 1]);
            if (speedList.Count != 0 && time < speedList[^1].time) AssertError("The time in speed line is not in nondecreasing order.");
            if (time > g_Tap.TimeJudge) AssertError("The time in speed line is out of range.");
            speedList.Add((time, speedRate));
        }
        g_Tap.SetMoveList(new BaseNoteMoveList(speedList, g_Tap.TimeJudge));
    }

    private void CaseTrail(Match match)
    {
        // @$"^\s*trail\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
        string[] moveLine = match.Groups[1].Value.Split(',');

        List<(float time, float speedRate)> moveList = new();
        for (int i = 0; i < moveLine.Length; i += 2)
        {
            // (time, y)
            float time = int.Parse(moveLine[i]) / 1000f;
            float y = float.Parse(moveLine[i + 1]);
            if (moveList.Count != 0 && time < moveList[^1].time) AssertError("The time in trail line is not in nondecreasing order.");
            if (time > g_Tap.TimeJudge) AssertError("The time in trail line is out of range.");
            moveList.Add((time, y));
        }
        g_Tap.SetMoveList(new BaseNoteMoveList(moveList, g_Tap.TimeJudge, false));
    }

    private void CaseTrack(Match match)
    {
        float timeStart = int.Parse(match.Groups[1].Value) / 1000f;
        float timeEnd = int.Parse(match.Groups[2].Value) / 1000f;
        float lPosEnd = float.Parse(match.Groups[3].Value);
        float rPosEnd = float.Parse(match.Groups[4].Value);

        // 构造一个完整的track
        g_Track = new(g_ID++, TrackType.Common, timeStart, timeEnd, lPosEnd, rPosEnd, true, false, false, g_Line);
        componentList.Add(g_Track);

        // 处理该track的所有修饰行
        bool isNewElement = false, hasPosLine = false;
        while (!isNewElement && ptr + 1 < chart.Length)
        {
            bool isValidLine = false;
            foreach (var pair in regexes)
            {
                var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
                if (nextMatch.Success)
                {
                    isValidLine = true;
                    if (TrackRegex.Contains(pair.Key))
                    {
                        ptr++;
                        switch (pair.Key)
                        {
                            case Regexes.LPos:
                                if (!hasPosLine)
                                {
                                    hasPosLine = true;
                                    CaseLPos(nextMatch, lPosEnd, rPosEnd);
                                }
                                else AssertError("Duplicate position property.");
                                break;
                            case Regexes.Tap:
                            case Regexes.TapS:
                                CaseTap(nextMatch);
                                break;
                            case Regexes.Hold:
                            case Regexes.HoldS:
                                CaseHold(nextMatch);
                                break;
                            case Regexes.Slide:
                            case Regexes.SlideS:
                                CaseSlide(nextMatch);
                                break;
                        }
                        break;
                    }
                    else if (BaseRegex.Contains(pair.Key))
                    {
                        isValidLine = true;
                        isNewElement = true;
                        break;
                    }

                    else AssertError("Property appears in a wrong place.");
                }
            }
            if (!isValidLine)
            {
                ptr++;
                AssertError("Find invalid content.");
            }
        }
    }

    private void CaseLPos(Match match, float lPosEnd, float rPosEnd)
    {
        // @$"^\s*lpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
        // @$"^\s*rpos\s*\(((?:\s*{uInt},\s*{Float},\s*{Curve},)*(?:\s*{uInt},\s*{Float},\s*{Curve}))\s*\)\s*;"
        // 这是一个双行修饰行，因此没有CaseRPos这个方法。
        string[] lPosLine = match.Groups[1].Value.Split(',');
        ptr++;
        Match rPosMatch = Regex.Match(chart[ptr], regexes[Regexes.RPos]);
        if (!rPosMatch.Success)
        {
            AssertError("Find lpos line not following rpos line.");
            return;
        }
        string[] rPosLine = Regex.Match(chart[ptr], regexes[Regexes.RPos]).Groups[1].Value.Split(',');

        List<(float time, float x, string curve)> lPosList = new(), rPosList = new();
        for (int i = 0; i < lPosLine.Length; i += 3)
        {
            // (time, pos, curve)
            float time = int.Parse(lPosLine[i]) / 1000f;
            float x = float.Parse(lPosLine[i + 1]);
            string curve = lPosLine[i + 2].Trim();
            if (lPosList.Count != 0 && time < lPosList[^1].time) AssertError("The time in pos line is not in nondecreasing order.");
            if (time < g_Track.TimeInstantiate || time > g_Track.TimeEnd) AssertError("The time in pos line is out of range.");
            lPosList.Add((time, x, curve));
        }
        for (int i = 0; i < rPosLine.Length; i += 3)
        {
            // (time, pos, curve)
            float time = int.Parse(rPosLine[i]) / 1000f;
            float x = float.Parse(rPosLine[i + 1]);
            string curve = rPosLine[i + 2].Trim();
            if (rPosList.Count != 0 && time < rPosList[^1].time) AssertError("The time in pos line is not in nondecreasing order.");
            if (time < g_Track.TimeInstantiate || time > g_Track.TimeEnd) AssertError("The time in pos line is out of range.");
            rPosList.Add((time, x, curve));
        }
        g_Track.LMoveList = new(lPosList, g_Track.TimeInstantiate, g_Track.TimeEnd, lPosEnd);
        g_Track.RMoveList = new(rPosList, g_Track.TimeInstantiate, g_Track.TimeEnd, rPosEnd);
    }

    private void CaseTap(Match match)
    {
        float timeJudge = int.Parse(match.Groups[1].Value) / 1000f;
        float speedRate = match.Groups.Count < 3 ? 1 : float.Parse(match.Groups[2].Value);
        if (timeJudge < g_Track.TimeInstantiate || timeJudge > g_Track.TimeEnd)
        {
            AssertError("Tnote's judge time is out of range of its track.");
            return;
        }

        // 构造一个完整的tap
        g_Tap = new(g_ID++, g_Track.Type.GetNoteType(), timeJudge, g_Track, speedRate);
        g_Track.Notes.AddItem(g_Tap);
        noteList[g_Tap.JudgeType].Add(g_Tap);
        componentList.Add(g_Tap);

        // 处理该tap的所有修饰行
        bool isNewElement = false, hasSpeedLine = false;
        while (!isNewElement && ptr + 1 < chart.Length)
        {
            bool isValidLine = false;
            foreach (var pair in regexes)
            {
                var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
                if (nextMatch.Success)
                {
                    isValidLine = true;
                    if (TNoteRegex.Contains(pair.Key))
                    {
                        ptr++;
                        switch (pair.Key)
                        {
                            case Regexes.Speed:
                                if (!hasSpeedLine)
                                {
                                    hasSpeedLine = true;
                                    CaseSpeed(nextMatch);
                                }
                                else AssertError("Duplicate movement property.");
                                break;
                            case Regexes.Trail:
                                if (!hasSpeedLine)
                                {
                                    hasSpeedLine = true;
                                    CaseTrail(nextMatch);
                                }
                                else AssertError("Duplicate movement property.");
                                break;
                        }
                        break;
                    }
                    else if (BaseRegex.Contains(pair.Key) || TrackRegex.Contains(pair.Key))
                    {
                        isValidLine = true;
                        isNewElement = true;
                        break;
                    }
                    else AssertError("Property appears in a wrong place.");
                }
            }
            if (!isValidLine)
            {
                ptr++;
                AssertError("Find invalid content.");
            }
        }
    }

    private void CaseSlide(Match match)
    {
        float timeJudge = int.Parse(match.Groups[1].Value) / 1000f;
        float speedRate = match.Groups.Count < 3 ? 1 : float.Parse(match.Groups[2].Value);
        if (timeJudge < g_Track.TimeInstantiate || timeJudge > g_Track.TimeEnd)
        {
            AssertError("Tnote's judge time is out of range of its track.");
            return;
        }

        // 构造一个完整的slide
        g_Slide = new(g_ID++, NoteType.Slide, timeJudge, g_Track, speedRate);
        g_Track.Notes.AddItem(g_Slide);
        noteList[g_Slide.JudgeType].Add(g_Slide);
        componentList.Add(g_Slide);

        // 处理该slide的所有修饰行
        //bool isNewElement = false, hasSpeedLine = false;
        //while (!isNewElement && ptr + 1 < chart.Length)
        //{
        //    bool isValidLine = false;
        //    foreach (var pair in regexes)
        //    {
        //        var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
        //        if (nextMatch.Success)
        //        {
        //            isValidLine = true;
        //            if (TNoteRegex.Contains(pair.Key))
        //            {
        //                ptr++;
        //                switch (pair.Key)
        //                {
        //                    case Regexes.Speed:
        //                        if (!hasSpeedLine)
        //                        {
        //                            hasSpeedLine = true;
        //                            CaseSpeed(nextMatch);
        //                        }
        //                        else AssertError("Duplicate movement property.");
        //                        break;
        //                    case Regexes.Trail:
        //                        if (!hasSpeedLine)
        //                        {
        //                            hasSpeedLine = true;
        //                            CaseTrail(nextMatch);
        //                        }
        //                        else AssertError("Duplicate movement property.");
        //                        break;
        //                }
        //                break;
        //            }
        //            else if (BaseRegex.Contains(pair.Key) || TrackRegex.Contains(pair.Key))
        //            {
        //                isValidLine = true;
        //                isNewElement = true;
        //                break;
        //            }
        //            else AssertError("Property appears in a wrong place.");
        //        }
        //    }
        //    if (!isValidLine)
        //    {
        //        ptr++;
        //        AssertError("Find invalid content.");
        //    }
        //}
    }

    private void CaseHold(Match match)
    {
        float timeJudge = int.Parse(match.Groups[1].Value) / 1000f;
        float timeEnd = int.Parse(match.Groups[2].Value) / 1000f;
        float speedRate = match.Groups.Count < 4 ? 1 : float.Parse(match.Groups[3].Value);
        if (timeJudge < g_Track.TimeInstantiate || timeJudge > g_Track.TimeEnd)
        {
            AssertError("Thold's judge time is out of range of its track.");
            return;
        }
        if (timeEnd < g_Track.TimeInstantiate || timeEnd > g_Track.TimeEnd)
        {
            AssertError("Thold's end time is out of range of its track.");
            return;
        }
        if (timeJudge >= timeEnd)
        {
            AssertError("Thold's end time is earlier than its judge time.");
            return;
        }

        // 构造一个完整的hold
        g_Hold = new(g_ID++, g_Track.Type.GetNoteType(true), timeJudge, g_Track, timeEnd, speedRate);
        g_Track.Notes.AddItem(g_Hold);
        componentList.Add(g_Hold);
        noteList[g_Hold.JudgeType].Add(g_Hold);

        // 处理该hold的所有修饰行
        bool isNewElement = false, hasSpeedLine = false, hasScaleLine = false;
        while (!isNewElement && ptr + 1 < chart.Length)
        {
            bool isValidLine = false;
            foreach (var pair in regexes)
            {
                var nextMatch = Regex.Match(chart[ptr + 1], pair.Value);
                if (nextMatch.Success)
                {
                    isValidLine = true;
                    if (THoldRegex.Contains(pair.Key))
                    {
                        ptr++;
                        switch (pair.Key)
                        {
                            case Regexes.Speed:
                                if (!hasSpeedLine)
                                {
                                    hasSpeedLine = true;
                                    CaseHoldSpeed(nextMatch);
                                }
                                else AssertError("Duplicate movement property.");
                                break;
                            case Regexes.Trail:
                                if (!hasSpeedLine)
                                {
                                    hasSpeedLine = true;
                                    CaseHoldTrail(nextMatch);
                                }
                                else AssertError("Duplicate movement property.");
                                break;
                            case Regexes.Scale:
                                if (!hasScaleLine)
                                {
                                    hasScaleLine = true;
                                    CaseScale(nextMatch);
                                }
                                else AssertError("Duplicate scale property.");
                                break;
                        }
                        break;
                    }
                    else if (BaseRegex.Contains(pair.Key) || TrackRegex.Contains(pair.Key))
                    {
                        isValidLine = true;
                        isNewElement = true;
                        break;
                    }
                    else AssertError("Property appears in a wrong place.");
                }
            }
            if (!isValidLine)
            {
                ptr++;
                AssertError("Find invalid content.");
            }
        }
    }

    private void CaseHoldSpeed(Match match)
    {
        // @$"^\s*speed\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
        string[] speedLine = match.Groups[1].Value.Split(',');

        List<(float time, float speedRate)> headSpeedList = new();
        List<(float time, float speedRate)> tailSpeedList = new() { (g_Hold.TimeJudge, 1) };
        for (int i = 0; i < speedLine.Length; i += 2)
        {
            // (time, speedRate)
            float time = int.Parse(speedLine[i]) / 1000f;
            float speedRate = float.Parse(speedLine[i + 1]);
            if (time < headSpeedList[^1].time) AssertError("The time in speed line is not in nondecreasing order.");
            if (time < g_Track.TimeInstantiate || time > g_Hold.TimeEnd) AssertError("The time in speed line is out of range.");
            if (time <= g_Hold.TimeJudge) headSpeedList.Add((time, speedRate));
            else tailSpeedList.Add((time, speedRate));
        }
        tailSpeedList[0] = (g_Hold.TimeJudge, headSpeedList[^1].speedRate);
        g_Hold.SetMoveList(new BaseNoteMoveList(headSpeedList, g_Hold.TimeJudge));
        g_Hold.SetScaleList(new BaseNoteMoveList(tailSpeedList, g_Hold.TimeEnd, g_Hold.TimeJudge));
    }

    private void CaseHoldTrail(Match match)
    {
        // @$"^\s*trail\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
        string[] moveLine = match.Groups[1].Value.Split(',');

        List<(float time, float y)> moveList = new();
        for (int i = 0; i < moveLine.Length; i += 2)
        {
            // (time, y)
            float time = int.Parse(moveLine[i]) / 1000f;
            float y = float.Parse(moveLine[i + 1]);
            if (moveList.Count != 0 && time < moveList[^1].time) AssertError("The time in trail line is not in nondecreasing order.");
            if (time > g_Hold.TimeJudge) AssertError("The time in trail line is out of range.");
            moveList.Add((time, y));
        }
        g_Hold.SetMoveList(new BaseNoteMoveList(moveList, g_Hold.TimeJudge, false));
    }

    private void CaseScale(Match match)
    {
        // @$"^\s*scale\s*\(((?:\s*{uInt},\s*{Float},)*(?:\s*{uInt},\s*{Float}))\s*\)\s*;
        string[] scaleLine = match.Groups[1].Value.Split(',');

        List<(float time, float y)> scaleList = new();
        for (int i = 0; i < scaleLine.Length; i += 2)
        {
            // (time, y)
            float time = int.Parse(scaleLine[i]) / 1000f;
            float scale = float.Parse(scaleLine[i + 1]);
            if (scaleList.Count != 0 && time < scaleList[^1].time) AssertError("The time in scale line is not in nondecreasing order.");
            if (time > g_Hold.TimeJudge) AssertError("The time in scale line is out of range.");
            scaleList.Add((time, scale));
        }
        g_Hold.SetScaleList(new BaseNoteMoveList(scaleList, g_Hold.TimeEnd, false, g_Hold.TimeInstantiate));
    }

    private void CaseCamera(Match match)
    {
        // Camera line read when finished relative functions.
    }

    private void CasePlot(Match match)
    {
        // Plot line read when finished relative functions.
    }

    private void CaseLayerName(Match match)
    {
        if (isLayerNameDone && isChartProcess) AssertError("Wrong layer line position.");
        string[] nameLine = match.Groups[1].Value.Split(',');

        List<string> names = new();

        // 读取名称并排序
        List<(int literal, string name)> layers = new();
        for (int i = 0; i < nameLine.Length; i += 2)
        {
            int literal = int.Parse(nameLine[i]);
            string name = nameLine[i + 1].Trim();
            if (literal < 3) AssertError("Illegal layer number.");
            layers.Add((literal, name));
        }
        layers.Sort((a, b) => a.literal.CompareTo(b.literal));
        for (int i = 0; i < layers.Count; i++)
        {
            layerLiteralToActual[layers[i].literal] = i + 3;
            names.Add(layers[i].name);
        }
        layerInfo.LayerNames = names;
        isLayerNameDone = true;
    }

    private void CaseLayerOrder(Match match)
    {
        if (isLayerOrderDone && isChartProcess) AssertError("Wrong layer line position.");
        string[] orderLine = match.Groups[1].Value.Split(',');
        List<int> layerOrder = new();

        // 读取各轨道的图层
        for (int i = 0; i < orderLine.Length; i++)
        {
            int num = int.Parse(orderLine[i]);
            if (num < 3) layerOrder.Add(num);
            else if (layerLiteralToActual.ContainsKey(num)) layerOrder.Add(layerLiteralToActual[num]);
            else layerOrder.Add(0);
        }
        layerInfo.TrackBelongings = layerOrder;
        isLayerOrderDone = true;
    }

    private void AssertError(string message)
    {
        Debug.LogError($"{message} Position: Line {ptr + 1}");
    }
    private void AssertError(Match match)
    {
        Debug.LogError($"Property is at a wrong place. Position: Line {ptr + 1}");
    }
}
