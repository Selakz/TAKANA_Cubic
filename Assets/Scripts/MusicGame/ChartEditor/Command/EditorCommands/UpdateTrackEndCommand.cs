public class UpdateTrackEndCommand : ICommand
{
    public string Name => $"ÐÞ¸ÄTrack: {track.Id}";

    private readonly Track track;
    private readonly float original;
    private readonly float updated;

    public UpdateTrackEndCommand(Track track, float updated)
    {
        if (updated >= 0 && updated > track.TimeInstantiate
         && updated > track.LMoveList[^2].time && updated > track.RMoveList[^2].time
         && (track.Notes.Count == 0 || updated > track.Notes["Judge", track.Notes.Count - 1].TimeJudge))
        {
            this.track = track;
            this.original = track.TimeEnd;
            this.updated = updated + 0.0001f;
        }
        else throw new System.Exception("Illegal updated track end time");
    }

    public void Do()
    {
        track.TimeEnd = updated;
        track.LMoveList.TimeEnd = updated;
        track.RMoveList.TimeEnd = updated;
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }

    public void Undo()
    {
        track.TimeEnd = original;
        track.LMoveList.TimeEnd = original;
        track.RMoveList.TimeEnd = original;
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }
}
