#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	/// <summary>
	/// Recursively clone the component and its descendant components.<br/><br/>
	/// A quite hacking practice!
	/// </summary>
	public class CloneComponentCommand : ICommand
	{
		public string Name => $"Clone Component {component.Id}";

		public IReadOnlyList<ChartComponent> ClonedComponents => clonedComponents;

		private readonly ChartInfo? chart;
		private readonly ChartComponent component;
		private readonly Action<ChartComponent>? postAction;
		private readonly List<ChartComponent> clonedComponents = new();

		public CloneComponentCommand(ChartComponent component, Action<ChartComponent>? postAction = null)
		{
			chart = component.BelongingChart;
			this.component = component;
			this.postAction = postAction;
		}

		public void Do()
		{
			if (clonedComponents.Count > 0)
			{
				clonedComponents[0].BelongingChart = chart;
				return;
			}

			var serialized = component.GetSerializationToken();
			if (chart is not null) chart.OnComponentAdded += OnComponentAdded;
			var cloned = ChartComponent.Deserialize(serialized, chart!);
			if (chart is not null) chart.OnComponentAdded -= OnComponentAdded;
			postAction?.Invoke(cloned);
			if (chart is not null)
			{
				foreach (var c in clonedComponents) c.Id = chart.NewId;
			}
		}

		private void OnComponentAdded(ChartComponent obj) => clonedComponents.Add(obj);

		public void Undo()
		{
			if (clonedComponents.Count > 0) clonedComponents[0].BelongingChart = null;
		}
	}
}