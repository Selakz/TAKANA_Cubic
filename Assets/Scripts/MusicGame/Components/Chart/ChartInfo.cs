using Takana3.Settings;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text;

/// <summary>
/// 通用谱面信息
/// </summary>
public class ChartInfo
{
    public float Offset { get; } = 0f;
    public Dictionary<JudgeType, MultiSortList<INote>> NoteDict { get; }
    public MultiSortList<IComponent> ComponentList { get; } // 同样包括Note在内
    public Dictionary<string, string> CustomInfo { get; }

    // 快速获取各类note的数量
    public int this[NoteType type]
    {
        get
        {
            int sum = 0;
            foreach (var pair in NoteDict)
            {
                foreach (var note in pair.Value)
                {
                    if (note.Type == type) sum++;
                }
            }
            return sum;
        }
    }
    public int NoteSum
    {
        get
        {
            int sum = 0;
            foreach (var pair in NoteDict)
            {
                sum += pair.Value.Count;
            }
            return sum;
        }
    }
    public int UNoteSum
    {
        get
        {
            int sum = 0;
            foreach (var type in NoteType.UTap.UNoteTypes())
            {
                sum += this[type];
            }
            return sum;
        }
    }
    public int TNoteSum
    {
        get
        {
            int sum = 0;
            foreach (var type in NoteType.Tap.TNoteTypes())
            {
                sum += this[type];
            }
            return sum;
        }
    }

    public ChartInfo(float offset, Dictionary<JudgeType, List<INote>> noteDict, MultiSortList<IComponent> componentList, Dictionary<string, string> customInfo)
    {
        Offset = offset;
        NoteDict = new();
        ComponentList = componentList;
        CustomInfo = new(customInfo);

        foreach (var pair in noteDict)
        {
            MultiSortList<INote> list = new(pair.Value);
            list.AddSort("Judge", (INote x, INote y) => x.TimeJudge.CompareTo(y.TimeJudge));
            list.AddSort("ID", (INote x, INote y) => x.Id.CompareTo(y.Id));
            NoteDict.Add(pair.Key, list);
        }
        ComponentList.AddSort("ID", (IComponent x, IComponent y) => x.Id.CompareTo(y.Id));
    }

    public ChartInfo(RawChartInfo rawChartInfo)
    {
        Offset = rawChartInfo.Offset;
        NoteDict = new(rawChartInfo.NoteDict);
        ComponentList = new();
        foreach (var component in rawChartInfo.ComponentList)
        {
            ComponentList.AddItem(component.Component);
        }
        CustomInfo = new(rawChartInfo.CustomInfo);
    }

    /// <summary>
    /// 在开始调用谱面前使用这个方法处理一些初始化事项
    /// </summary>
    public void Initialize(MusicSetting setting)
    {
        foreach (var component in ComponentList)
        {
            component.Initialize(setting);
        }
        foreach (var pair in NoteDict)
        {
            pair.Value.AddSort("Instantiate", (INote x, INote y) => x.TimeInstantiate.CompareTo(y.TimeInstantiate));
        }
        ComponentList.AddSort("Instantiate", (IComponent x, IComponent y) => x.TimeInstantiate.CompareTo(y.TimeInstantiate));
    }

    public RawChartInfo ToRawChartInfo()
    {
        return new(this);
    }

    /// <summary> 转换为Takana3简单语法的dlf谱面文件 </summary>
    public string ToSimpleDLF()
    {
        // TODO: 将构造下发到各个元件的ToString()中
        StringBuilder ret = new($"offset({Mathf.RoundToInt(Offset * 1000f)});\n");
        foreach (var pair in CustomInfo)
        {
            ret.AppendLine($"{pair.Key} : {pair.Value};");
        }
        ret.AppendLine("-");
        foreach (var component in ComponentList)
        {
            if (component is Track track)
            {
                // track本体
                float lPosEnd = track.LMoveList[^1].x;
                float rPosEnd = track.RMoveList[^1].x;
                ret.AppendLine($"track({Mathf.RoundToInt(track.TimeInstantiate * 1000f)}, {Mathf.RoundToInt(track.TimeEnd * 1000f)}, {lPosEnd}, {rPosEnd});");
                // track运动列表
                ret.AppendLine($"lpos{track.LMoveList}");
                ret.AppendLine($"rpos{track.RMoveList}");
                // track附属note
                foreach (var note in track.Notes)
                {
                    ret.AppendLine(
                        note switch
                        {
                            Tap tap => $"\ttap({Mathf.RoundToInt(tap.TimeJudge * 1000f)});",
                            Slide slide => $"\tslide({Mathf.RoundToInt(slide.TimeJudge * 1000f)});",
                            Hold hold => $"\thold({Mathf.RoundToInt(hold.TimeJudge * 1000f)}, {Mathf.RoundToInt(hold.TimeEnd * 1000f)});",
                            _ => string.Empty
                        }
                    );
                }
            }
        }
        return ret.ToString();
    }
}
