using System;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    // Serializable and Public
    public SelectMode SelectMode { get; set; } = SelectMode.OneByOne;
    public SelectTarget SelectTarget { get; set; } = SelectTarget.Note;
    public static SelectManager Instance => _instance;

    /// <summary> 只读不写 </summary>
    public MultiSortList<EditingTrack> SelectedTracks => _selectedTracks;
    /// <summary> 只读不写 </summary>
    public MultiSortList<EditingNote> SelectedNotes => _selectedNotes;

    // Private
    private readonly MultiSortList<EditingTrack> _selectedTracks = new();
    private readonly MultiSortList<EditingNote> _selectedNotes = new();

    // Static
    private static SelectManager _instance = null;

    // Defined Functions
    // 没有复用代码有点申必，但是反正就两种情况，暂时先这样吧
    public void Raycast(SelectTarget selectTarget)
    {
        if (!Camera.main.ContainsScreenPoint(Input.mousePosition)) return;
        switch (selectTarget)
        {
            case SelectTarget.Track: RaycastTrack(); break;
            case SelectTarget.Note: RaycastNote(); break;
            case SelectTarget.TurningPoint: RaycastTurningPoint(); break;
            default: break;
        }

    }

    public void UnselectAll()
    {
        foreach (var track in _selectedTracks) track.Unselect();
        _selectedTracks.Clear();
        foreach (var note in _selectedNotes) note.Unselect();
        _selectedNotes.Clear();
        TrackLineManager.Instance.Clear();
        EditPanelManager.Instance.AskForRender();
    }

    public void RaycastTrack()
    {
        SelectTarget = SelectTarget.Track;
        // TODO: 按下ctrl时的逻辑
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.Track.GetMask());
        if (raycastHits.Length == 0) { UnselectAll(); return; }
        switch (SelectMode)
        {
            case SelectMode.OneByOne:
                Array.Sort(raycastHits, (RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));
                if (_selectedTracks.Count == 1)
                {
                    for (int i = 0; i < raycastHits.Length; i++)
                    {
                        if (EditingLevelManager.Instance.RawChartInfo.GetTrack(GetTrackId(raycastHits[i])).IsSelected)
                        {
                            UnselectAll();
                            SelectTrack(GetTrackId(raycastHits[(i + 1) % raycastHits.Length]));
                            return;
                        }
                    }
                }
                UnselectAll();
                SelectTrack(GetTrackId(raycastHits[0]));
                break;
            case SelectMode.AllCasted or _:
                UnselectAll();
                foreach (var raycastHit in raycastHits)
                {
                    int id = GetTrackId(raycastHit);
                    SelectTrack(id);
                }
                break;
        }
    }

    public void SelectTrack(int id)
    {
        SelectTrack(EditingLevelManager.Instance.RawChartInfo.GetTrack(id));
    }

    public void SelectTrack(EditingTrack track)
    {
        track.Select();
        _selectedTracks.AddItem(track);
        TrackLineShownDetect();
        EditPanelManager.Instance.AskForRender();
    }

    public void UnselectTrack(int id)
    {
        for (int i = 0; i < _selectedTracks.Count; i++)
        {
            if (_selectedTracks[i].Id == id)
            {
                _selectedTracks[i].Unselect();
                _selectedTracks.RemoveItemAt(i);
                break;
            }
        }
        TrackLineShownDetect();
        EditPanelManager.Instance.AskForRender();
    }

    public void TrackLineShownDetect()
    {
        if (_selectedTracks.Count == 1) TrackLineManager.Instance.Decorate(_selectedTracks[0].Track);
        else TrackLineManager.Instance.Clear();
    }

    public void RaycastNote()
    {
        SelectTarget = SelectTarget.Note;
        // TODO: 按下ctrl时的逻辑
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.Note.GetMask());
        if (raycastHits.Length == 0) { UnselectAll(); return; }
        switch (SelectMode)
        {
            case SelectMode.OneByOne:
                Array.Sort(raycastHits, (RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));
                if (_selectedNotes.Count == 1)
                {
                    for (int i = 0; i < raycastHits.Length; i++)
                    {
                        if (EditingLevelManager.Instance.RawChartInfo.GetNote(GetNoteId(raycastHits[i])).IsSelected)
                        {
                            UnselectAll();
                            SelectNote(GetNoteId(raycastHits[(i + 1) % raycastHits.Length]));
                            return;
                        }
                    }
                }
                UnselectAll();
                SelectNote(GetNoteId(raycastHits[0]));
                break;
            case SelectMode.AllCasted or _:
                UnselectAll();
                foreach (var raycastHit in raycastHits)
                {
                    int id = GetNoteId(raycastHit);
                    SelectNote(id);
                }
                break;
        }
    }

    public void SelectNote(int id)
    {
        SelectNote(EditingLevelManager.Instance.RawChartInfo.GetNote(id));
    }

    public void SelectNote(EditingNote note)
    {
        note.Select();
        _selectedNotes.AddItem(note);
        EditPanelManager.Instance.AskForRender();
    }

    public void UnselectNote(int id)
    {
        for (int i = 0; i < _selectedNotes.Count; i++)
        {
            if (_selectedNotes[i].Id == id)
            {
                _selectedNotes[i].Unselect();
                _selectedNotes.RemoveItemAt(i);
                break;
            }
        }
        EditPanelManager.Instance.AskForRender();
    }

    private int GetTrackId(RaycastHit2D raycastHit) => raycastHit.transform.GetComponent<TrackController>().Info.Id;

    private int GetNoteId(RaycastHit2D raycastHit) => raycastHit.transform.GetComponent<BaseNoteController>().Info.Id;

    public void RaycastTurningPoint()
    {
        SelectTarget = SelectTarget.TurningPoint;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.TurningPoint.GetMask());
        if (raycastHits.Length == 0) return;
        else
        {
            var point = raycastHits[0].transform.parent.GetComponent<TurningPoint>();
            if (point != null) point.Select();
        }
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        _selectedTracks.AddSort("Instantiate", (EditingTrack x, EditingTrack y) => x.Track.TimeInstantiate.CompareTo(y.Track.TimeInstantiate));
        _selectedNotes.AddSort("Judge", (EditingNote x, EditingNote y) => x.Note.TimeJudge.CompareTo(y.Note.TimeJudge));
    }

    void Update()
    {
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.RaycastTrack))
        {
            Raycast(SelectTarget.Track);
        }
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.RaycastNote))
        {
            Raycast(SelectTarget.Note);
        }
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.RaycastTurningPoint))
        {
            Raycast(SelectTarget.TurningPoint);
        }
    }
}
