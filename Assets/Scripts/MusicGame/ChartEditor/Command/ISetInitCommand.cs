namespace MusicGame.ChartEditor.Command
{
	public interface ISetInitCommand : ICommand
	{
		/// <summary>
		/// You should always call this method before adding the command to any command manager.
		/// If it returns false, the command has problems, and you should not do it.
		/// </summary>
		public bool SetInit();
	}

	public interface ISetInitCommand<in T> : ICommand
	{
		/// <summary>
		/// You should always call this method before adding the command to any command manager.
		/// If it returns false, the command has problems, and you should not do it.
		/// </summary>
		public bool SetInit(T initItem);
	}
}