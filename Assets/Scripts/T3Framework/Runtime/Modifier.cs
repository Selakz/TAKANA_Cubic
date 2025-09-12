#nullable enable

using System;
using System.Collections.Generic;

namespace T3Framework.Runtime
{
	/// <summary> Note: Actions with larger priority will be executed later. </summary>
	public class Modifier<T>
	{
		private readonly Func<T> getter;
		private readonly Action<T> setter;
		private readonly Func<T, T> resetFunction;
		private readonly SortedList<int, Func<T, T>> functions = new();

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

		/// <summary>
		/// If duplicate registering a same priority, formerly registered function will be replaced.
		/// </summary>
		public void Register(Func<T, T> function, int priority)
		{
			functions[priority] = function;
			DoModify();
		}

		public void Unregister(int priority)
		{
			if (!functions.Remove(priority)) return;
			DoModify();
		}

		public void Update()
		{
			DoModify();
		}
	}
}