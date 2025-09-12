using System;
using System.Collections.Generic;
using System.Linq;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.EditingComponents;
using MusicGame.ChartEditor.Level;
using MusicGame.Components;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class DeleteComponentsCommand : ICommand
	{
		public string Name => $"Delete Components: {string.Join(',', components.Select(c => c.Id))}";

		public event Action OnRedo = delegate { };

		private readonly IComponent[] components;
		private EditingComponent[] deletingComponents;

		public DeleteComponentsCommand(IComponent component)
		{
			components = new[] { component };
		}

		public DeleteComponentsCommand(IEnumerable<IComponent> components)
		{
			this.components = components.ToArray();
		}

		public void Do()
		{
			var deleting = IEditingChartManager.Instance.RemoveComponents(components.Select(c => c.Id));
			deletingComponents ??= deleting.ToArray();
		}

		public void Undo()
		{
			IEditingChartManager.Instance.AddComponents(deletingComponents);
			OnRedo.Invoke();
		}
	}
}