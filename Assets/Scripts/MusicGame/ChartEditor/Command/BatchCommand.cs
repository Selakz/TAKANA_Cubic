#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace MusicGame.ChartEditor.Command
{
	public class BatchCommand : ICommand
	{
		public string Name { get; }

		public bool IsSkippable => Commands.Length == 0;

		public ICommand[] Commands { get; }

		public BatchCommand(IEnumerable<ICommand> commands, string description)
		{
			Commands = commands.ToArray();
			Name = description;
		}

		public void Do()
		{
			foreach (var c in Commands)
			{
				c.Do();
			}
		}

		public void Undo()
		{
			foreach (var c in Commands.Reverse())
			{
				c.Undo();
			}
		}
	}
}