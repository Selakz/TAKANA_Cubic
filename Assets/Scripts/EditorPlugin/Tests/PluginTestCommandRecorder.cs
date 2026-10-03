#nullable enable

using System;
using MusicGame.ChartEditor.Command;

namespace EditorPlugin.Tests
{
	/// <summary>
	/// Records what the editor's command manager does while a scenario runs, so that the C# side checks can assert
	/// on commits, undo and redo without reaching into the manager's private stacks. The counts mirror the manager's
	/// undo and redo lists: skippable commands are executed but never pushed, and the manager reports them as an add
	/// followed by a redo, which is ignored here.
	/// </summary>
	public sealed class PluginTestCommandRecorder : IDisposable
	{
		private readonly CommandManager commandManager;
		private readonly Action<ICommand> onAdd;
		private readonly Action<ICommand> onUndo;
		private readonly Action<ICommand> onRedo;

		private ICommand? pendingAdd;

		/// <summary> Number of commands added through the manager, i.e. the number of commits. </summary>
		public int BatchCount { get; private set; }

		public string? LastBatchName { get; private set; }

		public ICommand[] LastBatch { get; private set; } = Array.Empty<ICommand>();

		public int UndoCount { get; private set; }

		public int RedoCount { get; private set; }

		public PluginTestCommandRecorder(CommandManager commandManager)
		{
			this.commandManager = commandManager;
			onAdd = OnAdd;
			onUndo = OnUndo;
			onRedo = OnRedo;
			commandManager.OnAdd += onAdd;
			commandManager.OnUndo += onUndo;
			commandManager.OnRedo += onRedo;
		}

		public void Reset()
		{
			BatchCount = 0;
			LastBatchName = null;
			LastBatch = Array.Empty<ICommand>();
			UndoCount = 0;
			RedoCount = 0;
			pendingAdd = null;
		}

		public void Dispose()
		{
			commandManager.OnAdd -= onAdd;
			commandManager.OnUndo -= onUndo;
			commandManager.OnRedo -= onRedo;
		}

		private void OnAdd(ICommand command)
		{
			BatchCount++;
			LastBatchName = command.Name;
			LastBatch = command is BatchCommand batch ? batch.Commands : new[] { command };
			if (command.IsSkippable) return;

			pendingAdd = command;
			UndoCount++;
			RedoCount = 0;
		}

		private void OnUndo(ICommand command)
		{
			if (command.IsSkippable) return;
			UndoCount--;
			RedoCount++;
		}

		private void OnRedo(ICommand command)
		{
			if (command.IsSkippable) return;
			if (ReferenceEquals(pendingAdd, command))
			{
				// The manager reports the add of a fresh command as a redo as well.
				pendingAdd = null;
				return;
			}

			UndoCount++;
			RedoCount--;
		}
	}
}
