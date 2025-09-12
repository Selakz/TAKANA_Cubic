using System;

namespace T3Framework.Runtime
{
	public struct T3Time : IEquatable<T3Time>, IComparable<T3Time>
	{
		public int Milli { get; private set; }

		public float Second
		{
			get => Milli / 1000f;
			set => Milli = (int)(value * 1000f);
		}

		public T3Time(int milliseconds)
		{
			Milli = milliseconds;
		}

		public T3Time(float seconds)
		{
			Milli = (int)(seconds * 1000);
		}

		public static T3Time MinValue => new(int.MinValue);
		public static T3Time MaxValue => new(int.MaxValue);

		public static implicit operator T3Time(int milliseconds) => new(milliseconds);
		public static implicit operator int(T3Time time) => time.Milli;
		public static implicit operator T3Time(float seconds) => new(seconds);
		public static implicit operator float(T3Time time) => time.Second;

		public static bool operator ==(T3Time a, T3Time b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(T3Time a, T3Time b)
		{
			return !(a == b);
		}

		public static bool operator >(T3Time a, T3Time b)
		{
			return a.Milli > b.Milli;
		}

		public static bool operator <(T3Time a, T3Time b)
		{
			return a.Milli < b.Milli;
		}

		public static bool operator >=(T3Time a, T3Time b)
		{
			return a.Milli >= b.Milli;
		}

		public static bool operator <=(T3Time a, T3Time b)
		{
			return a.Milli <= b.Milli;
		}

		public static T3Time operator +(T3Time a, T3Time b)
		{
			return new T3Time(a.Milli + b.Milli);
		}

		public static T3Time operator -(T3Time a, T3Time b)
		{
			return new T3Time(a.Milli - b.Milli);
		}

		public bool Equals(T3Time other)
		{
			return Milli == other.Milli;
		}

		public override bool Equals(object obj)
		{
			return obj is T3Time other && Equals(other);
		}

		public override int GetHashCode()
		{
			return Milli;
		}

		public int CompareTo(T3Time other)
		{
			return Milli.CompareTo(other.Milli);
		}

		public override string ToString()
		{
			return Milli.ToString();
		}

		public static T3Time Parse(string s)
		{
			if (!s.Contains('.'))
			{
				try
				{
					return new T3Time(int.Parse(s));
				}
				catch
				{
					return new T3Time(float.Parse(s));
				}
			}

			return new T3Time(float.Parse(s));
		}
	}
}