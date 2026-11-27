#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;

namespace EditorPlugin.PluginSystem
{
	public sealed class StagingRegistry : IDisposable
	{
		private readonly List<(Func<ICommand> Factory, Action? OnDiscard)> entries = new();
		private readonly CommandManager commandManager;

		public bool HasPending => entries.Count > 0;

		public int FlushCount { get; private set; }

		public StagingRegistry(CommandManager commandManager)
		{
			this.commandManager = commandManager;
		}

		public void Add(Func<ICommand> commandFactory, Action? onDiscard = null) =>
			entries.Add((commandFactory, onDiscard));

		public bool Remove(Func<ICommand> commandFactory)
		{
			int index = entries.FindIndex(entry => entry.Factory == commandFactory);
			if (index < 0) return false;
			entries.RemoveAt(index);
			return true;
		}

		public void Discard()
		{
			if (entries.Count == 0) return;
			foreach (var entry in entries) entry.OnDiscard?.Invoke();
			entries.Clear();
		}

		public void Dispose() => entries.Clear();

		public void Flush()
		{
			if (entries.Count == 0) return;
			var commands = new List<ICommand>(entries.Count);
			commands.AddRange(entries.Select(entry => entry.Factory.Invoke()));
			commandManager.Add(new BatchCommand(commands, "Plugin edit"));
			entries.Clear();
			FlushCount++;
		}
	}
}