#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class UpdateComponentCommand : ICommand
	{
		public string Name => $"Update Component: {component.Id}";

		private readonly ChartComponent component;
		private readonly Action<ChartComponent> doAction;
		private readonly Action<ChartComponent> undoAction;

		public UpdateComponentCommand
			(ChartComponent component, Action<IChartModel> doAction, Action<IChartModel> undoAction)
		{
			this.component = component;
			this.doAction = c => c.UpdateModel(doAction);
			this.undoAction = c => c.UpdateModel(undoAction);
		}

		public UpdateComponentCommand
			(Action<ChartComponent> doAction, Action<ChartComponent> undoAction, ChartComponent component)
		{
			this.component = component;
			this.doAction = doAction;
			this.undoAction = undoAction;
		}

		public void Do() => doAction.Invoke(component);

		public void Undo() => undoAction.Invoke(component);
	}
}