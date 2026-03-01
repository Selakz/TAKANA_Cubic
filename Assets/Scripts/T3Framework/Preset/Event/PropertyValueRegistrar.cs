#nullable enable

using System;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace T3Framework.Preset.Event
{
	public class PropertyValueRegistrar<T> : IEventRegistrar
	{
		private readonly PropertyRegistrar<T> registrar;
		private readonly IEventRegistrar nestedRegistrar;

		private bool registered = false;

		public PropertyValueRegistrar(
			NotifiableProperty<T> property, Func<T, bool> predicate, IEventRegistrar registrar)
		{
			nestedRegistrar = registrar;
			this.registrar = new(property, value =>
			{
				if (!registered && predicate.Invoke(value))
				{
					registered = true;
					nestedRegistrar.Register();
				}
				else if (registered && !predicate.Invoke(value))
				{
					registered = false;
					nestedRegistrar.Unregister();
				}
			});
		}

		public PropertyValueRegistrar(NotifiableProperty<T> property, T targetValue, IEventRegistrar registrar)
			: this(property, value => Equals(value, targetValue), registrar)
		{
		}

		public void Register() => registrar.Register();

		public void Unregister()
		{
			registrar.Unregister();
			if (registered)
			{
				registered = false;
				nestedRegistrar.Unregister();
			}
		}
	}
}