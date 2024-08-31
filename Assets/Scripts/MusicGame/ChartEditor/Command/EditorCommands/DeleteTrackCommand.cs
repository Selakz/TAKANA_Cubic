public class DeleteTrackCommand : ICommand
{
    public string Name => $"É¾³ýTrack: {track.Id}";

    private readonly Track track;

    public DeleteTrackCommand(Track track)
    {
        this.track = track;
    }

    public void Do()
    {
        SelectManager.Instance.UnselectTrack(track.Id);
        foreach (var note in track.Notes)
        {
            EditingLevelManager.Instance.RawChartInfo.RemoveNote(note.Id);
            if (note.Controller != null) UnityEngine.Object.Destroy(note.Controller.gameObject);
        }
        EditingLevelManager.Instance.RawChartInfo.RemoveTrack(track.Id);
        if (track.Controller != null) UnityEngine.Object.Destroy(track.Controller.gameObject);
        EditingLevelManager.Instance.AskForResetField();
    }

    public void Undo()
    {
        EditingLevelManager.Instance.RawChartInfo.AddTrack(track);
        foreach (var note in track.Notes)
        {
            EditingLevelManager.Instance.RawChartInfo.AddNote(note);
        }
        EditingLevelManager.Instance.AskForResetField();
    }
}
