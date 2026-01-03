#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime.Event;

namespace MusicGame.ChartEditor.Command
{
	public readonly struct CommandFetchRegistrar<T, TData> : IEventRegistrar where T : ICommand
	{
		private readonly CommandRegistrar<T> singleRegistrar;
		private readonly CommandRegistrar<BatchCommand> batchRegistrar;

		public CommandFetchRegistrar(CommandManager commandManager, CommandProcess process,
			Func<T, IEnumerable<TData>> dataFetcher, Action<IEnumerable<TData>> action,
			Predicate<T>? commandFilter = null)
		{
			singleRegistrar = new CommandRegistrar<T>(commandManager, process, command =>
			{
				if (!(commandFilter?.Invoke(command) ?? true)) return;
				var data = dataFetcher.Invoke(command);
				action.Invoke(data);
			});
			batchRegistrar = new CommandRegistrar<BatchCommand>(commandManager, process, command =>
			{
				IEnumerable<TData> data = Enumerable.Empty<TData>();
				bool hasTCommand = false;
				foreach (var inner in command.Commands)
				{
					if (inner is T tCommand)
					{
						if (!(commandFilter?.Invoke(tCommand) ?? true)) continue;
						hasTCommand = true;
						data = data.Concat(dataFetcher.Invoke(tCommand));
					}
				}

				if (hasTCommand) action.Invoke(data);
			});
		}

		public CommandFetchRegistrar(CommandManager commandManager, CommandProcess process,
			Func<T, TData> dataFetcher, Action<IEnumerable<TData>> action,
			Predicate<T>? commandFilter = null)
		{
			singleRegistrar = new CommandRegistrar<T>(commandManager, process, command =>
			{
				if (!(commandFilter?.Invoke(command) ?? true)) return;
				var data = dataFetcher.Invoke(command);
				action.Invoke(Enumerable.Repeat(data, 1));
			});
			batchRegistrar = new CommandRegistrar<BatchCommand>(commandManager, process, command =>
			{
				IEnumerable<TData> data = Enumerable.Empty<TData>();
				bool hasTCommand = false;
				foreach (var inner in command.Commands)
				{
					if (inner is T tCommand)
					{
						if (!(commandFilter?.Invoke(tCommand) ?? true)) continue;
						hasTCommand = true;
						data = data.Concat(Enumerable.Repeat(dataFetcher.Invoke(tCommand), 1));
					}
				}

				if (hasTCommand) action.Invoke(data);
			});
		}

		public void Register()
		{
			singleRegistrar.Register();
			batchRegistrar.Register();
		}

		public void Unregister()
		{
			singleRegistrar.Unregister();
			batchRegistrar.Unregister();
		}
	}
}