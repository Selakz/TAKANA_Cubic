#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.ECS
{
	public class DatasetLifetimeRegistrar<T> : IEventRegistrar where T : IComponent
	{
		private readonly IReadOnlyDataset<T> dataset;
		private readonly Func<T, IEventRegistrar> registrarFactory;
		private readonly bool isCovering;

		private readonly Dictionary<T, IEventRegistrar> registrars = new();

		public DatasetLifetimeRegistrar(
			IReadOnlyDataset<T> dataset, Func<T, IEventRegistrar> registrarFactory, bool isCovering = false)
		{
			this.dataset = dataset;
			this.registrarFactory = registrarFactory;
			this.isCovering = isCovering;
		}

		public void OnGet(T component)
		{
			var registrar = registrarFactory.Invoke(component);
			registrar.Register();
			registrars[component] = registrar;
		}

		public void OnRelease(T component)
		{
			if (registrars.Remove(component, out var registrar)) registrar.Unregister();
		}

		public void Register()
		{
			dataset.OnDataAdded += OnGet;
			dataset.BeforeDataRemoved += OnRelease;
			if (isCovering)
			{
				foreach (var component in dataset)
				{
					OnGet(component);
				}
			}
		}

		public void Unregister()
		{
			dataset.OnDataAdded -= OnGet;
			dataset.BeforeDataRemoved -= OnRelease;
			foreach (var registrar in registrars.Values) registrar.Unregister();
			registrars.Clear();
		}
	}
}