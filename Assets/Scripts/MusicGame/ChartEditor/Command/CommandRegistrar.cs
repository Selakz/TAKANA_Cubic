#nullable enable

using System;
using T3Framework.Runtime.Event;

namespace MusicGame.ChartEditor.Command
{
	public enum CommandProcess
	{
		Add,
		Undo,
		Redo
	}

	public readonly struct CommandRegistrar<T> : IEventRegistrar where T : ICommand
	{
		private readonly CommandManager commandManager;
		private readonly CommandProcess process;
		private readonly Action<T> action;

		public CommandRegistrar(CommandManager commandManager, CommandProcess process, Action<T> action)
		{
			this.commandManager = commandManager;
			this.process = process;
			this.action = action;
		}

		public void Register()
		{
			switch (process)
			{
				case CommandProcess.Add:
					commandManager.OnAdd += DoAction;
					break;
				case CommandProcess.Undo:
					commandManager.OnUndo += DoAction;
					break;
				case CommandProcess.Redo:
					commandManager.OnRedo += DoAction;
					break;
			}
		}

		public void Unregister()
		{
			switch (process)
			{
				case CommandProcess.Add:
					commandManager.OnAdd -= DoAction;
					break;
				case CommandProcess.Undo:
					commandManager.OnUndo -= DoAction;
					break;
				case CommandProcess.Redo:
					commandManager.OnRedo -= DoAction;
					break;
			}
		}

		private void DoAction(ICommand command)
		{
			if (command is not T tCommand) return;
			action.Invoke(tCommand);
		}
	}
}