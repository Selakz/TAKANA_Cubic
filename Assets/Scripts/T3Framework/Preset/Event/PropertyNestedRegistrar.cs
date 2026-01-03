#nullable enable

using System;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace T3Framework.Preset.Event
{
	public class PropertyNestedRegistrar<T> : IEventRegistrar
	{
		private readonly PropertyRegistrar<T> registrar;
		private IEventRegistrar? nestedRegistrar;
		private readonly Action? initAction;

		public PropertyNestedRegistrar
			(NotifiableProperty<T> property, Func<T, IEventRegistrar> nestedFactory, Action? initAction = null)
		{
			this.initAction = initAction;
			registrar = new PropertyRegistrar<T>(property, () =>
			{
				initAction?.Invoke();
				nestedRegistrar?.Unregister();
				nestedRegistrar = null;
				var value = property.Value;
				if (value is null) return;
				nestedRegistrar = nestedFactory.Invoke(value);
				nestedRegistrar.Register();
			});
		}

		public void Register()
		{
			registrar.Register();
		}

		public void Unregister()
		{
			registrar.Unregister();
			nestedRegistrar?.Unregister();
			initAction?.Invoke();
		}
	}
}