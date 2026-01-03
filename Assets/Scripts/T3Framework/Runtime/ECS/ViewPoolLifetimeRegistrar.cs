#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public class ViewPoolLifetimeRegistrar<T> : IEventRegistrar where T : IComponent
	{
		private readonly IViewPool<T> viewPool;
		private readonly Func<PrefabHandler, IEventRegistrar> registrarFactory;
		private readonly Dictionary<PrefabHandler, IEventRegistrar> registrars = new();

		public ViewPoolLifetimeRegistrar
			(IViewPool<T> viewPool, Func<PrefabHandler, IEventRegistrar> registrarFactory)
		{
			this.viewPool = viewPool;
			this.registrarFactory = registrarFactory;
		}

		public void OnGet(object sender, PrefabHandler handler)
		{
			var registrar = registrarFactory.Invoke(handler);
			registrar.Register();
			registrars[handler] = registrar;
		}

		public void OnRelease(object sender, PrefabHandler handler)
		{
			if (registrars.Remove(handler, out var registrar)) registrar.Unregister();
		}

		public void Register()
		{
			viewPool.OnGet += OnGet;
			viewPool.OnRelease += OnRelease;
		}

		public void Unregister()
		{
			viewPool.OnGet -= OnGet;
			viewPool.OnRelease -= OnRelease;
			foreach (var registrar in registrars.Values) registrar.Unregister();
			registrars.Clear();
		}
	}
}