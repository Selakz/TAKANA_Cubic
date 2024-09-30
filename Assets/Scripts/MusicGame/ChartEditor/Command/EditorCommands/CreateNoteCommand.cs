/// <summary>
/// 只需要提供一个新的Note即可，如果是BaseNote在该指令里会更新所属Track的Notes属性
/// </summary>
public class CreateNoteCommand : ICommand
{
    public string Name => $"添加Note: {note.Id}";

    private readonly INote note;

    public CreateNoteCommand(INote note)
    {
        if (note is BaseNote baseNote)
        {
            if (baseNote.TimeJudge < baseNote.BelongingTrack.TimeInstantiate || baseNote.TimeJudge > baseNote.BelongingTrack.TimeEnd)
            {
                throw new System.Exception("illegal judge time");
            }
            if (baseNote is Hold hold && hold.TimeEnd > hold.BelongingTrack.TimeEnd)
            {
                throw new System.Exception("illegal hold end time");
            }
        }
        this.note = note;
    }

    public void Do()
    {
        EditingLevelManager.Instance.RawChartInfo.AddNote(note);
        if (note is BaseNote baseNote) baseNote.BelongingTrack.Notes.AddItem(baseNote);
        EditingLevelManager.Instance.AskForResetField();
    }

    public void Undo()
    {
        SelectManager.Instance.UnselectAll();
        EditingLevelManager.Instance.RawChartInfo.RemoveNote(note.Id);
        if (note is BaseNote baseNote)
        {
            baseNote.BelongingTrack.Notes.RemoveItem(baseNote);
            if (baseNote.Controller != null) UnityEngine.Object.Destroy(baseNote.Controller.gameObject);
        }
        EditingLevelManager.Instance.AskForResetField();
    }
}
