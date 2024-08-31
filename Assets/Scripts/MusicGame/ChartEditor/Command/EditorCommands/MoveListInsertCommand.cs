public class MoveListInsertCommand : ICommand
{
    public string Name => $"更改轨道运动列表";

    private readonly BaseTrackMoveList moveList;
    private readonly (float time, float x, string curve) item;

    public MoveListInsertCommand(BaseTrackMoveList moveList, (float time, float x, string curve) item)
    {
        if (item.time < moveList.TimeStart || item.time > moveList.TimeEnd)
        {
            throw new System.Exception("Illegal inserted move list item time");
        }
        this.moveList = moveList;
        this.item = item;
    }

    public void Do()
    {
        moveList.Insert(item);
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }

    public void Undo()
    {
        moveList.Remove(item);
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }
}
