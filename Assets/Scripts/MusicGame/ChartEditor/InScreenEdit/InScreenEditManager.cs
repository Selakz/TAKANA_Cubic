using System.Collections.Generic;
using UnityEngine;
using static Takana3.MusicGame.Values;

public class InScreenEditManager : MonoBehaviour
{
    // Serializable and Public
    [SerializeField] private Transform noteIndicator;

    public static InScreenEditManager Instance => _instance;
    public NoteType CreateType { get; set; } = NoteType.Tap;

    // Private

    // Static
    private static InScreenEditManager _instance;

    // Defined Functions
    private void CreateNote()
    {
        if (Camera.main.ContainsScreenPoint(Input.mousePosition) && SelectManager.Instance.SelectedTracks.Count == 1)
        {
            // 构建新Note的信息
            var editingTrack = SelectManager.Instance.SelectedTracks[0];
            if (editingTrack.Layer == 1)
            {
                HeaderMessage.Show("装饰层轨道禁止放置Note", HeaderMessage.MessageType.Info);
                return;
            }
            var track = editingTrack.Track;
            var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
            if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
            var time = GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed, gamePoint.y);
            if (time < track.TimeInstantiate || time > track.TimeEnd + 0.0005f)
            {
                HeaderMessage.Show("尝试创建的时间在所选轨道的时间范围外", HeaderMessage.MessageType.Warn);
                return;
            }
            BaseNote note;
            switch (CreateType)
            {
                case NoteType.Tap:
                    note = new Tap(EditingLevelManager.Instance.RawChartInfo.NewId, NoteType.Tap, time, track);
                    break;
                case NoteType.Slide:
                    note = new Slide(EditingLevelManager.Instance.RawChartInfo.NewId, NoteType.Slide, time, track);
                    break;
                case NoteType.Hold:
                    float timeEnd;
                    if (GridManager.Instance.IsTGridShow) timeEnd = GridManager.Instance.GetNearestTGridTime(time + 0.001f).ceiled;
                    else timeEnd = time + 1f;
                    if (timeEnd > track.TimeEnd)
                    {
                        if (time + 0.001f < track.TimeEnd) timeEnd = track.TimeEnd;
                        else { HeaderMessage.Show("尝试创建的时间在所选轨道的时间范围外", HeaderMessage.MessageType.Warn); return; }
                    }
                    note = new Hold(EditingLevelManager.Instance.RawChartInfo.NewId, NoteType.Hold, time, track, timeEnd);
                    break;
                default:
                    throw new System.Exception("Unhandled create type");
            };
            CommandManager.Instance.Add(new CreateNoteCommand(note));
        }
    }

    private static void CreateTrack()
    {
        if (Camera.main.ContainsScreenPoint(Input.mousePosition))
        {
            SelectManager.Instance.UnselectAll();
            // 构建新Track的信息
            var gamePoint = Camera.main.T3ScreenToGamePoint(Input.mousePosition);
            float inputX = gamePoint.x;
            if (GridManager.Instance.IsTGridShow) gamePoint = GridManager.Instance.GetAttachedGamePoint(gamePoint);
            var timeStart = GameYToTime(TimeProvider.Instance.ChartTime, EditingLevelManager.Instance.MusicSetting.Speed, gamePoint.y);
            float left = gamePoint.x - 1f, right = gamePoint.x + 1f;
            if (GridManager.Instance.IsXGridShow) (left, right) = GridManager.Instance.GetNearestXGridPos(inputX);
            float timeEnd;
            if (EditingLevelManager.Instance.GlobalSetting.IsInitialTrackLengthNotToEnd)
            {
                timeEnd = timeStart + EditingLevelManager.Instance.GlobalSetting.InitialTrackLength_Ms / 1000f;
            }
            else
            {
                timeEnd = TimeProvider.Instance.AudioToChartTime(TimeProvider.Instance.AudioLength);
            }
            Track track = new(EditingLevelManager.Instance.RawChartInfo.NewId, TrackType.Common, timeStart, timeEnd,
                left, right, true, false, false, EditingLevelManager.Instance.defaultJudgeLine);
            CommandManager.Instance.Add(new CreateTrackCommand(track));
            SelectManager.Instance.SelectTarget = SelectTarget.Track;
            SelectManager.Instance.SelectTrack(track.Id);
        }
    }

    private static void DeleteNote()
    {
        List<ICommand> commands = new();
        foreach (var note in SelectManager.Instance.SelectedNotes)
        {
            commands.Add(new DeleteNoteCommand(note.Note));
        }
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量删除Note"));
    }

    private static void DeleteTrack()
    {
        List<ICommand> commands = new();
        foreach (var track in SelectManager.Instance.SelectedTracks)
        {
            commands.Add(new DeleteTrackCommand(track.Track));
        }
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量删除Track"));
    }

    private static void NoteToNext()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                float newTime = note.Note.TimeJudge + 0.010f;
                commands.Add(new UpdateNoteCommand(note.Note, ConstructNewNote(note.Note, newTime)));
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Note"));
        EditPanelManager.Instance.Render();
    }

    private static void NoteToPrevious()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                float newTime = note.Note.TimeJudge - 0.010f;
                commands.Add(new UpdateNoteCommand(note.Note, ConstructNewNote(note.Note, newTime)));
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Note"));
        EditPanelManager.Instance.Render();
    }

    private static void NoteToNextBeat()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                float newTime = GridManager.Instance.GetNearestTGridTime(note.Note.TimeJudge + 0.001f).ceiled;
                commands.Add(new UpdateNoteCommand(note.Note, ConstructNewNote(note.Note, newTime)));
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Note"));
        EditPanelManager.Instance.Render();
    }

    private static void NoteToPreviousBeat()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                float newTime = GridManager.Instance.GetNearestTGridTime(note.Note.TimeJudge - 0.001f).floored;
                commands.Add(new UpdateNoteCommand(note.Note, ConstructNewNote(note.Note, newTime)));
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Note"));
        EditPanelManager.Instance.Render();
    }

    private static BaseNote ConstructNewNote(BaseNote note, float newTime)
    {
        return note switch
        {
            Tap tap => new Tap(tap.Id, NoteType.Tap, newTime, tap.BelongingTrack),
            Slide slide => new Slide(slide.Id, NoteType.Slide, newTime, slide.BelongingTrack),
            Hold hold => new Hold(hold.Id, NoteType.Hold, newTime, hold.BelongingTrack, newTime + hold.TimeEnd - hold.TimeJudge),
            _ => throw new System.Exception("Error selecting note type"),
        };
    }

    private static void HoldEndToNext()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                if (note is EditingHold hold)
                {
                    float newEnd = hold.Hold.TimeEnd + 0.010f;
                    commands.Add(new UpdateNoteCommand(note.Note, ConstructNewEndHold(hold.Hold, newEnd)));
                }
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        if (commands.Count == 0) return;
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Hold"));
        EditPanelManager.Instance.Render();
    }

    private static void HoldEndToPrevious()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                if (note is EditingHold hold)
                {
                    float newEnd = hold.Hold.TimeEnd - 0.010f;
                    commands.Add(new UpdateNoteCommand(note.Note, ConstructNewEndHold(hold.Hold, newEnd)));
                }
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        if (commands.Count == 0) return;
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Hold"));
        EditPanelManager.Instance.Render();
    }

    private static void HoldEndToNextBeat()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                if (note is EditingHold hold)
                {
                    float newEnd = GridManager.Instance.GetNearestTGridTime(hold.Hold.TimeEnd + 0.001f).ceiled;
                    commands.Add(new UpdateNoteCommand(note.Note, ConstructNewEndHold(hold.Hold, newEnd)));
                }
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        if (commands.Count == 0) return;
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Hold"));
        EditPanelManager.Instance.Render();
    }

    private static void HoldEndToPreviousBeat()
    {
        List<ICommand> commands = new();
        try
        {
            foreach (var note in SelectManager.Instance.SelectedNotes)
            {
                if (note is EditingHold hold)
                {
                    float newEnd = GridManager.Instance.GetNearestTGridTime(hold.Hold.TimeEnd - 0.001f).floored;
                    commands.Add(new UpdateNoteCommand(note.Note, ConstructNewEndHold(hold.Hold, newEnd)));
                }
            }
        }
        catch (System.Exception)
        {
            HeaderMessage.Show("移动失败，请检查时间边界", HeaderMessage.MessageType.Warn);
            return;
        }
        if (commands.Count == 0) return;
        CommandManager.Instance.Add(new BatchCommand(commands.ToArray(), "批量修改Hold"));
        EditPanelManager.Instance.Render();
    }

    private static Hold ConstructNewEndHold(Hold hold, float newEnd)
    {
        return new Hold(hold.Id, NoteType.Hold, hold.TimeJudge, hold.BelongingTrack, newEnd);
    }

    // System Functions
    void Awake()
    {
        _instance = this;
    }

    void Update()
    {
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.CreateNote))
        {
            CreateNote();
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.CreateTrack))
        {
            CreateTrack();
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.Delete))
        {
            switch (SelectManager.Instance.SelectTarget)
            {
                case SelectTarget.Track:
                    DeleteTrack();
                    break;
                case SelectTarget.Note:
                    DeleteNote();
                    break;
                default:
                    break;
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToNext))
        {
            if (SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                NoteToNext();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToPrevious))
        {
            if (SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                NoteToPrevious();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToNextBeat))
        {
            if (GridManager.Instance.IsTGridShow && SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                NoteToNextBeat();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.ToPreviousBeat))
        {
            if (GridManager.Instance.IsTGridShow && SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                NoteToPreviousBeat();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.HoldEndToNext))
        {
            if (SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                HoldEndToNext();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.HoldEndToPrevious))
        {
            if (SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                HoldEndToPrevious();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.HoldEndToNextBeat))
        {
            if (GridManager.Instance.IsTGridShow && SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                HoldEndToNextBeat();
            }
        }
        if (InputManager.Instance.IsHotkeyActionPressed(InputManager.Instance.Hotkeys.HoldEndToPreviousBeat))
        {
            if (GridManager.Instance.IsTGridShow && SelectManager.Instance.SelectTarget == SelectTarget.Note)
            {
                HoldEndToPreviousBeat();
            }
        }
    }
}
