using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Level;
using MusicGame.Components;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class UpdateComponentArg
	{
		public IComponent Component { get; }
		public Action<IComponent> DoAction { get; }
		public Action<IComponent> UndoAction { get; }

		public UpdateComponentArg(IComponent component, Action<IComponent> doAction, Action<IComponent> undoAction)
		{
			Component = component;
			DoAction = doAction;
			UndoAction = undoAction;
		}
	}

	public class UpdateComponentsCommand : ICommand
	{
		public string Name => $"Update Components: {string.Join(',', args.Select(c => c.Component.Id))}";

		private readonly UpdateComponentArg[] args;

		public UpdateComponentsCommand(UpdateComponentArg arg)
		{
			args = new[] { arg };
		}

		public UpdateComponentsCommand(IEnumerable<UpdateComponentArg> args)
		{
			this.args = args.ToArray();
		}

		public void Do()
		{
			foreach (var arg in args)
			{
				arg.DoAction(arg.Component);
			}

			IEditingChartManager.Instance.UpdateComponents(args.Select(c => c.Component.Id));
		}

		public void Undo()
		{
			foreach (var arg in args)
			{
				arg.UndoAction(arg.Component);
			}

			IEditingChartManager.Instance.UpdateComponents(args.Select(c => c.Component.Id));
		}
	}
}