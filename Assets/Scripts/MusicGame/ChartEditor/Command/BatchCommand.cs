using System.Linq;

public class BatchCommand : ICommand
{
    public string Name { get; private set; }

    private readonly ICommand[] commands = null;

    public BatchCommand(ICommand[] commands, string description)
    {
        if (commands.Length == 0) throw new System.Exception("Found no commands in a batch command.");
        this.commands = commands;
        Name = commands.Length == 1 ? commands[0].Name : description;
    }

    public void Do()
    {
        foreach (var c in commands)
        {
            c.Do();
        }
    }

    public void Undo()
    {
        foreach (var c in commands.Reverse())
        {
            c.Undo();
        }
    }
}
