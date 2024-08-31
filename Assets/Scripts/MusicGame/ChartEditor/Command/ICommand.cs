public interface ICommand
{
    public string Name { get; }

    public void Do();

    public void Undo();
}
