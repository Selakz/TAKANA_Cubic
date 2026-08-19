#nullable enable

using System;

namespace EditorPlugin.PluginSystem
{
	public sealed class PluginParamWrapper : IWrapper<object>
	{
		private readonly Type type;
		private object stored;

		public event Action? OnValueChanged;

		public PluginInstance Instance { get; }

		public PluginParamWrapper(Type type, PluginInstance instance)
		{
			this.type = type;
			stored = PluginParam.GetDefaultValue(type);
			Instance = instance;
		}

		public object value
		{
			get => stored;
			set
			{
				object converted = Convert.ChangeType(value, type);
				if (Equals(stored, converted)) return;
				stored = converted;
				OnValueChanged?.Invoke();
			}
		}

		public void Dispose()
		{
			OnValueChanged = null;
		}
	}
}