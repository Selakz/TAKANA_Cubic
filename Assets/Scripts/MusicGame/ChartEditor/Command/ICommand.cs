namespace MusicGame.ChartEditor.Command
{
	public interface ICommand
	{
		public string Name { get; }

		public bool IsSkippable => false;

		public void Do();

		public void Undo();
	}
}