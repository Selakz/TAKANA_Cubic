#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public class ViewPoolPluginRegistrar<T> : IEventRegistrar where T : IComponent
	{
		private readonly IEventRegistrar registrar;

		public ViewPoolPluginRegistrar(IViewPool<T> targetPool, IViewPool<T> pluginPool, string pluginName,
			Action<T>? onPluginAdded = null, Func<T, bool>? shouldAddPlugin = null)
		{
			pluginPool.IsGetActive = false;
			registrar = new ViewPoolLifetimeRegistrar<T>(targetPool, handler => new CustomRegistrar(
				() =>
				{
					var data = targetPool[handler];
					if (data is null || (!shouldAddPlugin?.Invoke(data) ?? true)) return;
					if (pluginPool.Add(data))
					{
						handler.AddPlugin(pluginName, pluginPool[data]!);
						onPluginAdded?.Invoke(data);
					}
				},
				() =>
				{
					var data = targetPool[handler];
					if (data is null) return;
					if (pluginPool.Remove(data)) handler.RemovePlugin(pluginName, pluginPool.DefaultTransform);
				}));
		}

		public void Register() => registrar.Register();

		public void Unregister() => registrar.Unregister();
	}

	public class ViewPoolPluginRegistrar<TTarget, TPlugin> : IEventRegistrar
		where TTarget : IComponent where TPlugin : IComponent
	{
		private readonly IEventRegistrar registrar;

		public ViewPoolPluginRegistrar(
			IViewPool<TTarget> targetPool,
			IViewPool<TPlugin> pluginPool,
			Func<TTarget, IEnumerable<(TPlugin, string)>> pluginDataGetter)
		{
			registrar = new ViewPoolLifetimeRegistrar<TTarget>(targetPool, handler => new CustomRegistrar(
				() =>
				{
					var data = targetPool[handler];
					if (data is null) return;
					foreach (var (plugin, name) in pluginDataGetter(data))
					{
						if (pluginPool.Add(plugin)) handler.AddPlugin(name, pluginPool[plugin]!);
					}
				},
				() =>
				{
					var data = targetPool[handler];
					if (data is null) return;
					foreach (var (plugin, name) in pluginDataGetter(data))
					{
						if (pluginPool.Remove(plugin))
						{
							handler.RemovePlugin(name, pluginPool.DefaultTransform);
						}
					}
				}));
		}

		public void Register() => registrar.Register();

		public void Unregister() => registrar.Unregister();
	}
}