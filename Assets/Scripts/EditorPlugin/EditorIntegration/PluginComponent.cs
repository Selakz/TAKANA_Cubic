#nullable enable

using System;
using EditorPlugin.PluginSystem;
using T3Framework.Runtime.ECS;

namespace EditorPlugin.EditorIntegration
{
	public class PluginComponent : IComponent<PluginInstance>
	{
		public PluginInstance Model { get; }

		public event EventHandler? OnComponentUpdated;

		public PluginComponent(PluginInstance model)
		{
			Model = model;
			foreach (var param in model.Params)
			{
				param.Value.OnValueChanged += UpdateNotify;
			}
		}

		public void UpdateNotify() => OnComponentUpdated?.Invoke(this, EventArgs.Empty);

		public void UpdateModel(Action<PluginInstance> action)
		{
			action.Invoke(Model);
			UpdateNotify();
		}
	}
}