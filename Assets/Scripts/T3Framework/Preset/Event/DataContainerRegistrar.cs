#nullable enable

using System.ComponentModel;
using T3Framework.Runtime.Event;
using T3Framework.Static.Event;

namespace T3Framework.Preset.Event
{
	public readonly struct DataContainerRegistrar<TData, TContainer>
		: IEventRegistrar where TContainer : NotifiableDataContainer<TData>
	{
		private readonly TContainer notifiableDataContainer;
		private readonly PropertyChangedEventHandler handler;

		public DataContainerRegistrar(TContainer notifiableDataContainer, PropertyChangedEventHandler handler)
		{
			this.notifiableDataContainer = notifiableDataContainer;
			this.handler = handler;
		}

		public void Register()
		{
			notifiableDataContainer.Property.PropertyChanged += handler;
			handler.Invoke(notifiableDataContainer, new PropertyChangedEventArgs("Value"));
		}

		public void Unregister()
		{
			notifiableDataContainer.Property.PropertyChanged -= handler;
		}
	}
}