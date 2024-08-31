/// <summary>
/// 只需要提供一个Note即可，如果是BaseNote在该指令里会更新所属Track的Notes属性
/// </summary>
public class DeleteNoteCommand : ICommand
{
    public string Name => $"删除Note: {note.Id}";

    private readonly INote note;

    public DeleteNoteCommand(INote note)
    {
        this.note = note;
    }

    public void Do()
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

    public void Undo()
    {
        EditingLevelManager.Instance.RawChartInfo.AddNote(note);
        if (note is BaseNote baseNote) baseNote.BelongingTrack.Notes.AddItem(baseNote);
        EditingLevelManager.Instance.AskForResetField();
    }
}
