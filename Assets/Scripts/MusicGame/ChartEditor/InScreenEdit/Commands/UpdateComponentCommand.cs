#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using UnityEngine;

namespace MusicGame.ChartEditor.InScreenEdit.Commands
{
	public class UpdateComponentCommand : ICommand
	{
		public string Name => $"Update Component: {Component.Id}";

		public ChartComponent Component { get; }

		private readonly Action<ChartComponent> doAction;
		private readonly Action<ChartComponent> undoAction;

		public UpdateComponentCommand
			(ChartComponent component, Action<IChartModel> doAction, Action<IChartModel> undoAction)
		{
			Component = component;
			this.doAction = c => c.UpdateModel(doAction);
			this.undoAction = c => c.UpdateModel(undoAction);
		}

		public UpdateComponentCommand
			(Action<ChartComponent> doAction, Action<ChartComponent> undoAction, ChartComponent component)
		{
			Component = component;
			this.doAction = doAction;
			this.undoAction = undoAction;
		}

		public void Do() => doAction.Invoke(Component);

		public void Undo() => undoAction.Invoke(Component);
	}

	public class UpdateComponentCommand<TValue> : ICommand
	{
		public string Name => $"Update Component: {Component.Id}";

		public ChartComponent Component { get; }

		private readonly Func<ChartComponent, TValue> getter;
		private readonly Action<ChartComponent, TValue> setter;
		private readonly TValue value;
		private TValue? oldValue;
		private bool hasDone = false;

		public UpdateComponentCommand(ChartComponent component,
			Func<ChartComponent, TValue> getter, Action<ChartComponent, TValue> setter, TValue value)
		{
			Component = component;
			this.getter = getter;
			this.setter = setter;
			this.value = value;
		}

		public void Do()
		{
			oldValue = getter.Invoke(Component);
			setter.Invoke(Component, value);
			Component.UpdateNotify();
			hasDone = true;
		}

		public void Undo()
		{
			if (!hasDone)
			{
				Debug.LogError("UpdateComponentCommand.Undo: command has not been done yet.");
				return;
			}

			setter.Invoke(Component, oldValue);
			Component.UpdateNotify();
			oldValue = default;
			hasDone = false;
		}

		public static ICommand Model<TModel>(ChartComponent component,
			Func<TModel, TValue> getter, Action<TModel, TValue> setter, TValue value) where TModel : IChartModel
		{
			if (component.Model is not TModel)
			{
				Debug.LogWarning($"Target component's model is not {typeof(TModel)}, doing nothing.");
				return EmptyCommand.Instance;
			}

			return new UpdateComponentCommand<TValue>(component,
				c => getter.Invoke((TModel)c.Model),
				(c, v) => setter.Invoke((TModel)c.Model, v),
				value);
		}
	}
}