#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace T3Framework.Preset.Event
{
	public readonly struct PropertyRegistrar<T> : IEventRegistrar
	{
		private readonly NotifiableProperty<T> property;
		private readonly PropertyChangedEventHandler handler;

		public PropertyRegistrar(NotifiableProperty<T> property, PropertyChangedEventHandler handler)
		{
			this.property = property;
			this.handler = handler;
		}

		public void Register()
		{
			property.PropertyChanged += handler;
			handler.Invoke(property, new PropertyChangedEventArgs("Value"));
		}

		public void Unregister()
		{
			property.PropertyChanged -= handler;
		}
	}
}