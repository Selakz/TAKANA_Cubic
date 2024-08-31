/// <summary>
/// һ������ṩ������Note��IdӦ��ͬ
/// </summary>
public class UpdateNoteCommand : ICommand
{
    public string Name => $"�޸�Note: {original.Id}";

    private readonly INote original;
    private readonly INote updated;

    public UpdateNoteCommand(INote original, INote updated)
    {
        if (updated is BaseNote note)
        {
            if (note.TimeJudge < note.BelongingTrack.TimeInstantiate || note.TimeJudge > note.BelongingTrack.TimeEnd) throw new System.Exception("illegal note's judge time");
            if (note is Hold hold && (hold.TimeEnd <= hold.TimeJudge || hold.TimeEnd > hold.BelongingTrack.TimeEnd)) throw new System.Exception("illegal hold's end time");
        }
        this.original = original;
        this.updated = updated;
    }

    // ������BatchCommand����û��Ҫ
    public void Do()
    {
        // delete
        SelectManager.Instance.UnselectNote(original.Id);
        EditingLevelManager.Instance.RawChartInfo.RemoveNote(original.Id);
        if (original is BaseNote baseNote)
        {
            baseNote.BelongingTrack.Notes.RemoveItem(baseNote);
            if (baseNote.Controller != null) UnityEngine.Object.Destroy(baseNote.Controller.gameObject);
        }
        // create
        EditingLevelManager.Instance.RawChartInfo.AddNote(updated);
        if (updated is BaseNote newBaseNote) newBaseNote.BelongingTrack.Notes.AddItem(newBaseNote);
        SelectManager.Instance.SelectNote(updated.Id);
        EditingLevelManager.Instance.AskForResetFieldTo(TimeProvider.Instance.ChartTime);
        EditPanelManager.Instance.AskForRender();
    }

    public void Undo()
    {
        // delete
        SelectManager.Instance.UnselectNote(updated.Id);
        EditingLevelManager.Instance.RawChartInfo.RemoveNote(updated.Id);
        if (updated is BaseNote baseNote)
        {
            baseNote.BelongingTrack.Notes.RemoveItem(baseNote);
            if (baseNote.Controller != null) UnityEngine.Object.Destroy(baseNote.Controller.gameObject);
        }
        // create
        EditingLevelManager.Instance.RawChartInfo.AddNote(original);
        if (original is BaseNote oldBaseNote) oldBaseNote.BelongingTrack.Notes.AddItem(oldBaseNote);
        SelectManager.Instance.SelectNote(original.Id);
        EditingLevelManager.Instance.AskForResetFieldTo(TimeProvider.Instance.ChartTime);
        EditPanelManager.Instance.AskForRender();
    }
}
