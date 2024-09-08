using System.Collections.Generic;
using System.Linq;
using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;

public class RawChartInfo
{
    public float Offset { get => _offset; set { _offset = value; if (TimeProvider.Instance != null) TimeProvider.Instance.Offset = _offset; } }
    public Dictionary<JudgeType, MultiSortList<INote>> NoteDict { get; }
    public MultiSortList<EditingComponent> ComponentList { get; } // 同样包括Note在内
    public Dictionary<string, string> CustomInfo { get; }
    public int NewId
    {
        get => _newId++;
        set { if (value < _newId) throw new System.Exception("Wrongly assinged new id"); _newId = value; }
    }

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

    private float _offset = 0f;

    private int _newId = 0;

    public RawChartInfo()
    {
        Offset = 0f;
        NoteDict = new();
        ComponentList = new();
        CustomInfo = new();
    }
    public RawChartInfo(ChartInfo chartInfo)
    {
        Offset = chartInfo.Offset;
        NoteDict = new(chartInfo.NoteDict);
        ComponentList = new();
        int maxId = 0;
        foreach (var component in chartInfo.ComponentList)
        {
            ComponentList.AddItem(EditingComponent.AutoWrapByType(component));
            maxId = Mathf.Max(maxId, component.Id); // 可以保证chartInfo中的ID是从0开始没有空缺的
        }
        NewId = maxId + 1;
        CustomInfo = new(chartInfo.CustomInfo);
    }

    // 经典CRUD
    /// <summary> 只是把track添加到列表中。添加附属note的操作在对应command中进行 </summary>
    public void AddTrack(Track track)
    {
        ComponentList.AddItem(EditingComponent.AutoWrapByType(track));
    }

    /// <summary> 只是把track从列表中移除。删除附属note的操作在对应command中进行 </summary>
    public void RemoveTrack(int id)
    {
        EditingTrack track = GetTrack(id);
        ComponentList.RemoveItem(track);
    }

    public void AddNote(INote note)
    {
        ComponentList.AddItem(EditingComponent.AutoWrapByType(note));
        NoteDict[note.Type.GetJudgeType()].AddItem(note);
    }

    public void RemoveNote(int id)
    {
        EditingNote note = GetNote(id);
        ComponentList.RemoveItem(note);
        foreach (var n in NoteDict[note.Note.Type.GetJudgeType()])
        {
            if (n.Id == id)
            {
                NoteDict[note.Note.Type.GetJudgeType()].RemoveItem(n);
                break;
            }
        }
    }

    public EditingTrack GetTrack(int id)
    {
        foreach (var component in from component in ComponentList
                                  where component != null && component.Id == id
                                  select component)
        {
            Assert.AreEqual(SelectTarget.Track, component.Type);
            return component as EditingTrack;
        }
        return null;
    }

    public EditingNote GetNote(int id)
    {
        foreach (var component in from component in ComponentList
                                  where component != null && component.Id == id
                                  select component)
        {
            Assert.AreEqual(SelectTarget.Note, component.Type);
            return component as EditingNote;
        }
        return null;
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
        ComponentList.AddSort("Instantiate", (EditingComponent x, EditingComponent y) => x.TimeInstantiate.CompareTo(y.TimeInstantiate));
    }

    public ChartInfo ToChartInfo()
    {
        return new(this);
    }
}
