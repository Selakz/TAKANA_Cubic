#nullable enable

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

		public T Value
		{
			get => innerValue;
			set
			{
				if (!EqualityComparer<T>.Default.Equals(innerValue, value))
				{
					innerValue = value;
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