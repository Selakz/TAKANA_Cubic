namespace MusicGame.ChartEditor.Command
{
	public class EmptyCommand : ICommand
	{
		public static EmptyCommand Instance { get; } = new();

		public string Name => string.Empty;

		public bool IsSkippable => true;

		public void Do()
		{
		}

		public void Undo()
		{
		}
	}
}