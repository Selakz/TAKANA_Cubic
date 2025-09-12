using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.Level;
using MusicGame.Components;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class AddComponentsCommand : ICommand
	{
		public string Name => $"Add Components: {string.Join(',', components.Select(c => c.Id))}";

		public event Action OnRedo = delegate { };

		private readonly IComponent[] components;

		public AddComponentsCommand(IComponent component)
		{
			components = new[] { component };
		}

		public AddComponentsCommand(IEnumerable<IComponent> components)
		{
			this.components = components.ToArray();
		}

		public void Do()
		{
			IEditingChartManager.Instance.AddComponents(components);
		}

		public void Undo()
		{
			IEditingChartManager.Instance.RemoveComponents(components.Select(component => component.Id));
			OnRedo.Invoke();
		}
	}
}