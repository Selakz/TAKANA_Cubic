#nullable enable

using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class AddComponentCommand : ICommand
	{
		public string Name => $"Add Component: {model.GetType().Name}";

		public ChartComponent? Component { get; private set; }

		private readonly ChartInfo chart;
		private readonly IChartModel model;
		private readonly ChartComponent? parent;
		private (int, string?)? identifiers;

		public AddComponentCommand(ChartInfo chart, IChartModel model, ChartComponent? parent)
		{
			this.chart = chart;
			this.model = model;
			this.parent = parent;
		}

		public void Do()
		{
			if (Component is null) Component = chart.AddComponent(model);
			else Component.BelongingChart = chart;

			if (identifiers != null) (Component.Id, Component.Name) = identifiers.Value;
			if (parent is not null) Component.SetParent(parent);
		}

		public void Undo()
		{
			if (Component is not null)
			{
				chart.RemoveComponent(Component);
				identifiers = (Component.Id, Component.Name);
			}
		}
	}
}