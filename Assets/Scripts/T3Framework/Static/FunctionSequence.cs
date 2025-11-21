#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace T3Framework.Static
{
	public class FunctionSequence
	{
		private readonly SortedList<int, (Func<bool>, Action<int>?)> priorityFunctions =
			new(Comparer<int>.Create((a, b) => b - a));

		/// <summary> Register a function to the sequence. </summary>
		/// <param name="priority"> Function with larger priority will be called first. </param>
		/// <param name="function"> Returns a bool, indicating whether to continue calling the following functions. </param>
		/// <param name="notify"> If the sequence is interrupted, the following notifies will ba called with the priority of whom interrupt the sequence. </param>
		public void Register(int priority, Func<bool> function, Action<int>? notify = null)
		{
			priorityFunctions[priority] = (function, notify);
		}

		public bool Unregister(int priority)
		{
			return priorityFunctions.Remove(priority);
		}

		public void Invoke()
		{
			var stopPriority = 0;
			var shouldContinue = true;
			foreach (var (priority, tuple) in priorityFunctions)
			{
				if (shouldContinue)
				{
					Debug.Log($"invoke {priority}'s function");
					shouldContinue = tuple.Item1.Invoke();
					if (!shouldContinue) stopPriority = priority;
				}
				else
				{
					Debug.Log($"invoke {priority}'s action");
					tuple.Item2?.Invoke(stopPriority);
				}
			}
		}
	}
}