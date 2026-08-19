#nullable enable

using System;
using MusicGame.ChartEditor.Command;

namespace EditorPlugin.PluginSystem
{
	public class ValueWrapper<T> : IWrapper<T>
	{
		private readonly Func<T> getter;
		private readonly Action<T> setter;
		private readonly StagingRegistry? registry;
		private T stagedValue;
		private bool dirty;

		public ValueWrapper(Func<T> getter, Action<T> setter, StagingRegistry? registry = null)
		{
			this.getter = getter;
			this.setter = setter;
			this.registry = registry;
			stagedValue = default!;
		}

		public T value
		{
			get => getter.Invoke();
			set
			{
				if (registry is null)
				{
					setter.Invoke(value);
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

		public ICommand Factory()
		{
			var commandValue = stagedValue;
			dirty = false;
			stagedValue = default!;
			return new UpdateValueCommand<T>(getter, setter, commandValue);
		}

		public void Dispose() => registry?.Remove(Factory);
	}
}