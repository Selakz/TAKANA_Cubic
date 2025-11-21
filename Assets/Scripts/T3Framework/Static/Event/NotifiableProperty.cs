#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace T3Framework.Static.Event
{
	public interface INotifiableProperty : INotifyPropertyChanged, IDisposable
	{
		public object GenericValue { get; set; }

		public object? LastGenericValue { get; }

		public void ForceNotify();
	}

	public sealed class NotifiableProperty<T> : INotifiableProperty
	{
		private T innerValue;
		private readonly HashSet<NotifiableProperty<T>> connectors = new();

		public event PropertyChangedEventHandler? PropertyChanged;

		public bool IsLastAssignmentNotified { get; private set; } = false;

		public Func<T, T> Clamp { get; set; } = x => x;

		public Func<T, T, bool> Comparer { get; set; } = EqualityComparer<T>.Default.Equals;

		public T Value
		{
			get => innerValue;
			set
			{
				IsLastAssignmentNotified = false;
				if (!Comparer.Invoke(innerValue, value))
				{
					LastValue = innerValue;
					innerValue = Clamp(value);
					IsLastAssignmentNotified = true;
					OnPropertyChanged();
				}
			}
		}

		public T? LastValue { get; private set; }

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

		public object? LastGenericValue => LastValue;

		public void ForceNotify()
		{
			IsLastAssignmentNotified = true;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
		}

		public void AddUpNotify()
		{
			if (!IsLastAssignmentNotified) ForceNotify();
		}

		private void OnOtherPropertyChanged(object o, PropertyChangedEventArgs args)
		{
			var other = (NotifiableProperty<T>)o;
			Value = other.Value;
		}

		/// <summary>
		/// On connect, the caller's value will override the callee.
		/// </summary>
		public bool Connect(NotifiableProperty<T> other)
		{
			if (!connectors.Add(other) || !other.connectors.Add(this)) return false;
			other.PropertyChanged += OnOtherPropertyChanged;
			PropertyChanged += other.OnOtherPropertyChanged;
			other.Value = Value;
			return true;
		}

		public bool Disconnect(NotifiableProperty<T> other)
		{
			if (!connectors.Remove(other) || !other.connectors.Remove(this)) return false;
			other.PropertyChanged -= OnOtherPropertyChanged;
			PropertyChanged -= other.OnOtherPropertyChanged;
			return true;
		}

		public void DisconnectAll()
		{
			foreach (var connector in connectors)
			{
				connector.connectors.Remove(this);
				connector.PropertyChanged -= OnOtherPropertyChanged;
				PropertyChanged -= connector.OnOtherPropertyChanged;
			}

			connectors.Clear();
		}

		public void Dispose()
		{
			PropertyChanged = null;
			DisconnectAll();
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