#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;

namespace EditorPlugin.PluginSystem
{
	public sealed class StagingRegistry : IDisposable
	{
		private readonly List<Func<ICommand>> commandFactories = new();
		private readonly CommandManager commandManager;

		public bool HasPending => commandFactories.Count > 0;

		public StagingRegistry(CommandManager commandManager)
		{
			this.commandManager = commandManager;
		}

		public void Add(Func<ICommand> commandFactory) => commandFactories.Add(commandFactory);

		public bool Remove(Func<ICommand> commandFactory) => commandFactories.Remove(commandFactory);

		public void Dispose() => commandFactories.Clear();

		public void Flush()
		{
			if (commandFactories.Count == 0) return;
			var commands = new List<ICommand>(commandFactories.Count);
			commands.AddRange(commandFactories.Select(factory => factory.Invoke()));
			commandManager.Add(new BatchCommand(commands, "Plugin edit"));
			commandFactories.Clear();
		}
	}
}