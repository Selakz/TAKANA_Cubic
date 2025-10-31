#nullable enable

using T3Framework.Runtime;

namespace T3Framework.Static.Event
{
	public abstract class NotifiableDataContainer<T> : T3MonoBehaviour
	{
		// Serializable and Public
		public abstract T InitialValue { get; }

		public NotifiableProperty<T> Property
		{
			get
			{
				property ??= new(InitialValue);
				return property;
			}
		}

		// Private
		private NotifiableProperty<T>? property;
	}
}