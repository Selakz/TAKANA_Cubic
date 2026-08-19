#nullable enable

using System;
using MusicGame.ChartEditor.Command;
using MusicGame.ChartEditor.InScreenEdit.Commands;
using MusicGame.Gameplay.Chart;
using MusicGame.Models;

namespace EditorPlugin.PluginSystem
{
	public interface IWrapper<T> : IDisposable
	{
		// ReSharper disable once InconsistentNaming
		public T value { get; set; }
	}

	public class PropertyWrapper<TModel, TValue> : IWrapper<TValue> where TModel : IChartModel
	{
		protected readonly ChartComponent component;
		protected readonly StagingRegistry? registry;

		private readonly Func<TModel, TValue> getter;
		private readonly Action<TModel, TValue> setter;
		private TValue stagedValue;
		private bool dirty;

		public PropertyWrapper(ChartComponent component,
			Func<TModel, TValue> getter, Action<TModel, TValue> setter, StagingRegistry? registry)
		{
			this.component = component;
			this.getter = getter;
			this.setter = setter;
			this.registry = registry;
			stagedValue = default!;
		}

		public TValue value
		{
			get => getter.Invoke((TModel)component.Model);
			set
			{
				if (registry is null)
				{
					setter.Invoke((TModel)component.Model, value);
					return;
				}

				if (!dirty)
				{
					registry.Add(Factory);
					dirty = true;
				}

				stagedValue = value;
			}
		}

		protected virtual ICommand Factory()
		{
			var commandValue = stagedValue;
			dirty = false;
			stagedValue = default!;
			return UpdateComponentCommand<TValue>.Model(component, getter, setter, commandValue);
		}

		public void Dispose() => registry?.Remove(Factory);
	}
}