#nullable enable

using System;
using T3Framework.Runtime.Log;

namespace MusicGame.ChartEditor.Command
{
	public class LogCommand : ICommand
	{
		public string Name => $"Logging {module}_{content}_{type}";

		public bool IsSkippable => true;

		private readonly string module;
		private readonly string content;
		private readonly Enum type;

		public LogCommand(string module, string content, Enum type)
		{
			this.module = module;
			this.content = content;
			this.type = type;
		}

		public void Do()
		{
			T3Logger.Log(module, content, type);
		}

		public void Undo()
		{
		}
	}
}