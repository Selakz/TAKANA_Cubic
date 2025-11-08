#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Static.Event;

namespace T3Framework.Runtime.Event
{
	public abstract class NotifiableDataContainer<T> : T3MonoBehaviour
	{
		// Serializable and Public
		public abstract T InitialValue { get; }

		protected virtual Func<T, T, bool> Comparer => EqualityComparer<T>.Default.Equals;

		public NotifiableProperty<T> Property
		{
			get
			{
				property ??= new(InitialValue)
				{
					Comparer = Comparer
				};
				return property;
			}
		}

		// Private
		private NotifiableProperty<T>? property;
	}
}