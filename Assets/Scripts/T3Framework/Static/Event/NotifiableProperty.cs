#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace T3Framework.Static.Event
{
	public interface INotifiableProperty : INotifyPropertyChanged
	{
		public object GenericValue { get; set; }

		public void ForceNotify();
	}

	public sealed class NotifiableProperty<T> : INotifiableProperty
	{
		private T innerValue;

		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IsLastAssignmentNotified { get; private set; } = false;

		public Func<T, T, bool> Comparer { get; set; } = EqualityComparer<T>.Default.Equals;

		public T Value
		{
			get => innerValue;
			set
			{
				IsLastAssignmentNotified = false;
				if (!Comparer.Invoke(innerValue, value))
				{
					innerValue = value;
					IsLastAssignmentNotified = true;
					OnPropertyChanged();
				}
			}
		}

		public NotifiableProperty(T initialValue)
		{
			innerValue = initialValue;
		}

		public object GenericValue
		{
			get => Value!;
			set
			{
				if (value is T t) Value = t;
			}
		}

		public void ForceNotify()
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
		}

		public void AddUpNotify()
		{
			if (!IsLastAssignmentNotified) ForceNotify();
		}

		public static implicit operator T(NotifiableProperty<T> property)
		{
			return property.Value;
		}

		private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}