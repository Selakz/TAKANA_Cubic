public class UpdateTrackStartCommand : ICommand
{
    public string Name => $"ĞŞ¸ÄTrack: {track.Id}";

    private readonly Track track;
    private readonly float original;
    private readonly float updated;

    public UpdateTrackStartCommand(Track track, float updated)
    {
        if (updated >= 0 && updated < track.TimeEnd
         && updated < track.LMoveList[1].time && updated < track.RMoveList[1].time
         && (track.Notes.Count == 0 || updated < track.Notes["Judge", 0].TimeJudge))
        {
            this.track = track;
            this.original = track.TimeInstantiate;
            this.updated = updated - 0.0001f;
        }
        else throw new System.Exception("Illegal updated track start time");
    }

    public void Do()
    {
        track.TimeInstantiate = updated;
        track.LMoveList.TimeStart = updated;
        track.RMoveList.TimeStart = updated;
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }

    public void Undo()
    {
        track.TimeInstantiate = original;
        track.LMoveList.TimeStart = original;
        track.RMoveList.TimeStart = original;
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }
}
