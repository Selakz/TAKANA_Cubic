using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectManager : MonoBehaviour
{
    // Serializable and Public
    public SelectMode SelectMode { get; set; } = SelectMode.OneByOne;
    public SelectTarget SelectTarget { get; set; } = SelectTarget.Note;
    public static SelectManager Instance => _instance;

    /// <summary> ÷ª∂¡≤ª–¥ </summary>
    public MultiSortList<EditingTrack> SelectedTracks => _selectedTracks;
    /// <summary> ÷ª∂¡≤ª–¥ </summary>
    public MultiSortList<EditingNote> SelectedNotes => _selectedNotes;

    // Private
    private readonly MultiSortList<EditingTrack> _selectedTracks = new();
    private readonly MultiSortList<EditingNote> _selectedNotes = new();

    // Static
    private static SelectManager _instance = null;

    // Defined Functions
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

    public void RaycastMulti(SelectTarget selectTarget)
    {
        if (!Camera.main.ContainsScreenPoint(Input.mousePosition)) return;
        switch (selectTarget)
        {
            case SelectTarget.Track: RaycastTrackMulti(); break;
            case SelectTarget.Note: RaycastNoteMulti(); break;
            case SelectTarget.TurningPoint: RaycastTurningPointMulti(); break;
            default: break;
        }
    }

    public void UnselectAll()
    {
        UnselectAllTracks();
        UnselectAllNotes();
    }

    /* Track */
    public void RaycastTrack()
    {
        SelectTarget = SelectTarget.Track;
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

    public void RaycastTrackMulti()
    {
        SelectTarget = SelectTarget.Track;
        UnselectAllNotes();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.Track.GetMask());
        if (raycastHits.Length == 0) return;
        switch (SelectMode)
        {
            case SelectMode.OneByOne:
                Array.Sort(raycastHits, (RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    int id = GetTrackId(raycastHits[i]);
                    if (EditingLevelManager.Instance.RawChartInfo.GetTrack(id).IsSelected)
                    {
                        UnselectTrack(id);
                        if (raycastHits.Length != 1) SelectTrack(GetTrackId(raycastHits[(i + 1) % raycastHits.Length]));
                        return;
                    }
                }
                SelectTrack(GetTrackId(raycastHits[0]));
                break;
            case SelectMode.AllCasted or _:
                foreach (var raycastHit in raycastHits)
                {
                    int id = GetTrackId(raycastHit);
                    if (EditingLevelManager.Instance.RawChartInfo.GetTrack(id).IsSelected) UnselectTrack(id);
                    else SelectTrack(id);
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
        track.IsSelected = true;
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
                _selectedTracks[i].IsSelected = false;
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

    public void UnselectAllTracks()
    {
        foreach (var track in _selectedTracks) track.IsSelected = false;
        _selectedTracks.Clear();
        TrackLineManager.Instance.Clear();
        EditPanelManager.Instance.AskForRender();
    }

    /* Note */
    public void RaycastNote()
    {
        SelectTarget = SelectTarget.Note;
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

    public void RaycastNoteMulti()
    {
        SelectTarget = SelectTarget.Note;
        UnselectAllTracks();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.Note.GetMask());
        if (raycastHits.Length == 0) return;
        switch (SelectMode)
        {
            case SelectMode.OneByOne:
                Array.Sort(raycastHits, (RaycastHit2D x, RaycastHit2D y) => x.distance.CompareTo(y.distance));
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    int id = GetNoteId(raycastHits[i]);
                    if (EditingLevelManager.Instance.RawChartInfo.GetNote(id).IsSelected)
                    {
                        UnselectNote(id);
                        if (raycastHits.Length != 1) SelectNote(GetNoteId(raycastHits[(i + 1) % raycastHits.Length]));
                        return;
                    }
                }
                SelectNote(GetNoteId(raycastHits[0]));
                break;
            case SelectMode.AllCasted or _:
                foreach (var raycastHit in raycastHits)
                {
                    int id = GetNoteId(raycastHit);
                    if (EditingLevelManager.Instance.RawChartInfo.GetNote(id).IsSelected) UnselectNote(id);
                    else SelectNote(id);
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
        note.IsSelected = true;
        _selectedNotes.AddItem(note);
        EditPanelManager.Instance.AskForRender();
    }

    public void UnselectNote(int id)
    {
        for (int i = 0; i < _selectedNotes.Count; i++)
        {
            if (_selectedNotes[i].Id == id)
            {
                _selectedNotes[i].IsSelected = false;
                _selectedNotes.RemoveItemAt(i);
                break;
            }
        }
        EditPanelManager.Instance.AskForRender();
    }

    public void UnselectAllNotes()
    {
        foreach (var note in _selectedNotes) note.IsSelected = false;
        _selectedNotes.Clear();
        EditPanelManager.Instance.AskForRender();
    }

    private int GetTrackId(RaycastHit2D raycastHit) => raycastHit.transform.GetComponent<TrackController>().Info.Id;

    private int GetNoteId(RaycastHit2D raycastHit) => raycastHit.transform.GetComponent<BaseNoteController>().Info.Id;

    /* TurningPoint */
    public void RaycastTurningPoint()
    {
        SelectTarget = SelectTarget.TurningPoint;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.TurningPoint.GetMask());
        if (raycastHits.Length == 0) return;
        else
        {
            EventManager.Trigger(EventManager.EventName.TurningPointUnselect);
            var point = raycastHits[0].transform.parent.GetComponent<TurningPoint>();
            if (point != null) point.IsSelected = true;
        }
    }

    public void RaycastTurningPointMulti()
    {
        SelectTarget = SelectTarget.TurningPoint;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D[] raycastHits = Physics2D.RaycastAll(ray.origin, ray.direction, float.MaxValue, SelectTarget.TurningPoint.GetMask());
        if (raycastHits.Length == 0) return;
        else
        {
            var point = raycastHits[0].transform.parent.GetComponent<TurningPoint>();
            if (point != null) point.IsSelected = !point.IsSelected;
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
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.RaycastTrackMulti))
        {
            RaycastMulti(SelectTarget.Track);
        }
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.RaycastNoteMulti))
        {
            RaycastMulti(SelectTarget.Note);
        }
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.RaycastTurningPointMulti))
        {
            RaycastMulti(SelectTarget.TurningPoint);
        }
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.Copy))
        {
            switch (SelectTarget)
            {
                case SelectTarget.Note:
                    List<BaseNote> notes = new();
                    foreach (var editingNote in _selectedNotes) notes.Add(editingNote.Note);
                    CopyPasteManager.Instance.CopyBaseNotes(notes);
                    break;
                case SelectTarget.Track:
                    List<Track> tracks = new();
                    foreach (var track in _selectedTracks) tracks.Add(track.Track);
                    CopyPasteManager.Instance.CopyTracks(tracks);
                    break;
                case SelectTarget.TurningPoint:
                    break;
                default:
                    break;
            }

        }
        else if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.Cut))
        {
            switch (SelectTarget)
            {
                case SelectTarget.Note:
                    List<BaseNote> notes = new();
                    foreach (var editingNote in _selectedNotes) notes.Add(editingNote.Note);
                    CopyPasteManager.Instance.CopyBaseNotes(notes);
                    List<ICommand> deleNoteCommands = new();
                    foreach (var note in _selectedNotes)
                    {
                        deleNoteCommands.Add(new DeleteNoteCommand(note.Note));
                    }
                    CommandManager.Instance.Add(new BatchCommand(deleNoteCommands.ToArray(), "ºÙ«–Note"));
                    break;
                case SelectTarget.Track:
                    List<Track> tracks = new();
                    foreach (var track in _selectedTracks) tracks.Add(track.Track);
                    CopyPasteManager.Instance.CopyTracks(tracks);
                    List<ICommand> deleTrackCommands = new();
                    foreach (var track in _selectedTracks)
                    {
                        deleTrackCommands.Add(new DeleteTrackCommand(track.Track));
                    }
                    CommandManager.Instance.Add(new BatchCommand(deleTrackCommands.ToArray(), "ºÙ«–Track"));
                    break;
                case SelectTarget.TurningPoint:
                    break;
                default:
                    break;
            }
        }
    }
}
