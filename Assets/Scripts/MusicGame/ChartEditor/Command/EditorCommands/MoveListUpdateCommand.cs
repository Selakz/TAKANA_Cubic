public class MoveListUpdateCommand : ICommand
{
    public string Name => $"更改轨道运动列表";

    private readonly BaseTrackMoveList moveList;
    private readonly (float time, float x, string curve) original;
    private readonly (float time, float x, string curve) updated;

    public MoveListUpdateCommand(BaseTrackMoveList moveList, (float time, float x, string curve) original, (float time, float x, string curve) updated)
    {
        if (moveList.IndexOf(original) < 0)
        {
            throw new System.Exception("Invalid original move list item when updating");
        }
        if (updated.time < moveList.TimeStart || updated.time > moveList.TimeEnd)
        {
            throw new System.Exception("Illegal updated move list item time");
        }
        this.moveList = moveList;
        this.original = original;
        this.updated = updated;
    }

    public void Do()
    {
        int index = moveList.IndexOf(original);
        moveList[index] = updated;
        TrackLineManager.Instance.RemoveSelectedItem(moveList, original);
        TrackLineManager.Instance.AddSelectedItem(moveList, updated);
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }

    public void Undo()
    {
        int index = moveList.IndexOf(updated);
        moveList[index] = original;
        TrackLineManager.Instance.RemoveSelectedItem(moveList, updated);
        TrackLineManager.Instance.AddSelectedItem(moveList, original);
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }
}
