#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace T3Framework.Runtime
{
	/// <summary>
	/// Note: Actions with larger priority will be executed later. <br/>
	/// </summary>
	public class Modifier<T>
	{
		private readonly Func<T> getter;
		private readonly Action<T> setter;
		private readonly Func<T, T> resetFunction;
		private readonly SortedList<int, Func<T, T>> functions = new();
		private bool isModified = false;

		public T Value => getter.Invoke();

		private void DoModify()
		{
			var value = Value;
			value = resetFunction.Invoke(value);
			foreach (var function in functions.Values)
			{
				value = function.Invoke(value);
			}

			setter.Invoke(value);
		}

		public Modifier(Func<T> getter, Action<T> setter, Func<T, T> resetFunction)
		{
			this.getter = getter;
			this.setter = setter;
			this.resetFunction = resetFunction;
			DoModify();
		}

		public Modifier(Func<T> getter, Action<T> setter, T defaultValue) : this(getter, setter, _ => defaultValue)
		{
		}

		public Modifier(Func<T> getter, Action<T> setter)
		{
			var defaultValue = getter.Invoke();
			this.getter = getter;
			this.setter = setter;
			resetFunction = _ => defaultValue;
			DoModify();
		}

		/// <summary>
		/// If duplicate registering a same priority, formerly registered function will be replaced.
		/// </summary>
		public void Register(Func<T, T> function, int priority, bool isLazy = false)
		{
			functions[priority] = function;
			isModified = true;
			if (!isLazy) DoModify();
		}

		public void Assign(T value, int priority, bool isLazy = false) => Register(_ => value, priority, isLazy);

		public void Unregister(int priority, bool isLazy = false)
		{
			if (!functions.Remove(priority)) return;
			isModified = true;
			if (!isLazy) DoModify();
		}

		public void Update()
		{
			if (!isModified) return;
			DoModify();
		}

		public void Clear() => functions.Clear();

		public override string ToString()
		{
			StringBuilder sb = new("Modifier:\n");
			var value = Value;
			value = resetFunction.Invoke(value);
			sb.AppendLine($"reset -> {resetFunction.Invoke(value)}");
			foreach (var (priority, function) in functions)
			{
				value = function.Invoke(value);
				sb.AppendLine($"{priority} -> {value}");
			}

			return sb.ToString();
		}
	}
}