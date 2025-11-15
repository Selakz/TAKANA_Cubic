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

		protected virtual Func<T, T> Clamp => x => x;

		protected virtual Func<T, T, bool> Comparer => EqualityComparer<T>.Default.Equals;

		public NotifiableProperty<T> Property
		{
			get
			{
				return property ??= new(InitialValue)
				{
					Clamp = Clamp,
					Comparer = Comparer
				};
			}
		}

		// Private
		private NotifiableProperty<T>? property;

		// System Functions
		protected override void OnDestroy()
		{
			base.OnDestroy();
			property?.Dispose();
		}
	}
}