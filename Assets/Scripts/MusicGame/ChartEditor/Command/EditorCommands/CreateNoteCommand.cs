/// <summary>
/// ֻ��Ҫ�ṩһ���µ�Note���ɣ������BaseNote�ڸ�ָ������������Track��Notes����
/// </summary>
public class CreateNoteCommand : ICommand
{
    public string Name => $"���Note: {note.Id}";

    private readonly INote note;

    public CreateNoteCommand(INote note)
    {
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
