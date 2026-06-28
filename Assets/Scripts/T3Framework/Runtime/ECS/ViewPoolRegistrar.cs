#nullable enable

using System;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public class ViewPoolRegistrar<T> : IEventRegistrar where T : IComponent
	{
		public enum RegisterTarget
		{
			Create,
			Get,
			Release,
			Destroy
		}

		private readonly IViewPool<T> viewPool;
		private readonly RegisterTarget target;
		private readonly EventHandler<PrefabHandler> handler;

		public ViewPoolRegistrar(IViewPool<T> viewPool, RegisterTarget target, EventHandler<PrefabHandler> handler)
		{
			this.viewPool = viewPool;
			this.target = target;
			this.handler = handler;
		}

		public ViewPoolRegistrar(IViewPool<T> viewPool, RegisterTarget target, Action<PrefabHandler> handler)
			: this(viewPool, target, (_, value) => handler.Invoke(value))
		{
		}

		public void Register()
		{
			switch (target)
			{
				case RegisterTarget.Create:
					viewPool.OnCreate += handler;
					break;
				case RegisterTarget.Get:
					viewPool.OnGet += handler;
					break;
				case RegisterTarget.Release:
					viewPool.OnRelease += handler;
					break;
				case RegisterTarget.Destroy:
					viewPool.OnDestroy += handler;
					break;
			}
		}

		public void Unregister()
		{
			switch (target)
			{
				case RegisterTarget.Create:
					viewPool.OnCreate -= handler;
					break;
				case RegisterTarget.Get:
					viewPool.OnGet -= handler;
					break;
				case RegisterTarget.Release:
					viewPool.OnRelease -= handler;
					break;
				case RegisterTarget.Destroy:
					viewPool.OnDestroy -= handler;
					break;
			}
		}
	}
}