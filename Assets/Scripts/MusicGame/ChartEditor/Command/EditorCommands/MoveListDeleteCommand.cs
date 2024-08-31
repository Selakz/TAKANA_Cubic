public class MoveListDeleteCommand : ICommand
{
    public string Name => $"更改轨道运动列表";

    private readonly BaseTrackMoveList moveList;
    private readonly (float time, float x, string curve) item;

    public MoveListDeleteCommand(BaseTrackMoveList moveList, (float time, float x, string curve) item)
    {
        if (moveList.IndexOf(item) < 0)
        {
            throw new System.Exception("Invalid move list item when removing");
        }
        this.moveList = moveList;
        this.item = item;
    }

    public void Do()
    {
        moveList.Remove(item);
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }

    public void Undo()
    {
        moveList.Insert(item);
        EditingLevelManager.Instance.AskForResetField();
        EditPanelManager.Instance.AskForRender();
    }
}
