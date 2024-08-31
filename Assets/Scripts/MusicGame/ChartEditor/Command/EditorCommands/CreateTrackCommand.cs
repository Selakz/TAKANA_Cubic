/// <summary>
/// ���ṩ��Track�Լ���Notes���������
/// </summary>
public class CreateTrackCommand : ICommand
{
    public string Name => $"���Track: {track.Id}";

    private readonly Track track;

    public CreateTrackCommand(Track track)
    {
        this.track = track;
    }

    public void Do()
    {
        EditingLevelManager.Instance.RawChartInfo.AddTrack(track);
        foreach (var note in track.Notes)
        {
            EditingLevelManager.Instance.RawChartInfo.AddNote(note);
        }
        EditingLevelManager.Instance.ResetField();
    }

    public void Undo()
    {
        SelectManager.Instance.UnselectAll();
        foreach (var note in track.Notes)
        {
            EditingLevelManager.Instance.RawChartInfo.RemoveNote(note.Id);
            if (note.Controller != null) UnityEngine.Object.Destroy(note.Controller.gameObject);
        }
        EditingLevelManager.Instance.RawChartInfo.RemoveTrack(track.Id);
        if (track.Controller != null) UnityEngine.Object.Destroy(track.Controller.gameObject);
        EditingLevelManager.Instance.AskForResetField();
    }
}
