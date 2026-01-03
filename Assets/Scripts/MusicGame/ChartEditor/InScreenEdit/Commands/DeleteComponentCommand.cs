#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class DeleteComponentCommand : ICommand
	{
		public string Name => $"Delete Component: {RootComponent.Id}";

		public ChartComponent RootComponent { get; }

		private readonly ChartInfo? chart;
		private readonly ChartComponent? parent;

		public DeleteComponentCommand(ChartComponent component)
		{
			chart = component.BelongingChart;
			parent = component.Parent;
			RootComponent = component;
		}

		public void Do()
		{
			RootComponent.BelongingChart = null;
		}

		public void Undo()
		{
			RootComponent.BelongingChart = chart;
			RootComponent.SetParent(parent);
		}
	}
}