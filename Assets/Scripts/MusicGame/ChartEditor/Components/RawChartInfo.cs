using System.Collections.Generic;
using Takana3.Settings;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

public class RawChartInfo
{
    public float Offset { get => _offset; set { _offset = value; if (TimeProvider.Instance != null) TimeProvider.Instance.Offset = _offset; } }
    public Dictionary<JudgeType, MultiSortList<INote>> NoteDict { get; }
    public MultiSortList<EditingComponent> ComponentList { get; } // ͬ������Note����
    public Dictionary<string, string> CustomInfo { get; }
    public int NewId
    {
        get => _newId++;
        set { if (value < _newId) throw new System.Exception("Wrongly assinged new id"); _newId = value; }
    }

    // ���ٻ�ȡ����note������
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
            maxId = Mathf.Max(maxId, component.Id); // ���Ա�֤chartInfo�е�ID�Ǵ�0��ʼû�п�ȱ��
        }
        NewId = maxId + 1;
        CustomInfo = new(chartInfo.CustomInfo);
    }

    // ����CRUD
    /// <summary> ֻ�ǰ�track��ӵ��б��С���Ӹ���note�Ĳ����ڶ�Ӧcommand�н��� </summary>
    public void AddTrack(Track track)
    {
        var editingTrack = EditingComponent.AutoWrapByType(track);
        (editingTrack as EditingTrack).Layer = TrackLayerManager.Instance == null ? 0 : TrackLayerManager.Instance.SelectedLayer;
        ComponentList.AddItem(editingTrack);
    }

    /// <summary> ֻ�ǰ�track���б����Ƴ���ɾ������note�Ĳ����ڶ�Ӧcommand�н��� </summary>
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
    /// �ڿ�ʼ��������ǰʹ�������������һЩ��ʼ������
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

    public void SetLayers(LayerInfo layerInfo)
    {
        int i = 0;
        foreach (var component in ComponentList)
        {
            if (component is EditingTrack track)
            {
                if (layerInfo.TrackBelongings.Count > i)
                {
                    track.Layer = layerInfo.TrackBelongings[i];
                    i++;
                }
                else track.Layer = 0;
            }
        }
    }

    public List<int> GetTrackBelongings()
    {
        List<int> list = new();
        foreach (var component in ComponentList)
        {
            if (component is EditingTrack track)
            {
                list.Add(track.Layer);
            }
        }
        return list;
    }

    public ChartInfo ToChartInfo()
    {
        return new(this);
    }
}
