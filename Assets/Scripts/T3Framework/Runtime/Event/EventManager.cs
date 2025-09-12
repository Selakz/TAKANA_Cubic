using System;
using System.Collections.Generic;
using UnityEngine.Events;

// Modified from StarryFramework. I think it has improvement space but whatever.
namespace T3Framework.Runtime.Event
{
	#region EventBase

	internal interface IEvent
	{
	}

	internal class EventInfo : IEvent
	{
		public readonly UnityEvent Event = new();
	}

	internal class EventInfo<T> : IEvent
	{
		public readonly UnityEvent<T> Event = new();
	}

	internal class EventInfo<T1, T2> : IEvent
	{
		public readonly UnityEvent<T1, T2> Event = new();
	}

	internal class EventInfo<T1, T2, T3> : IEvent
	{
		public readonly UnityEvent<T1, T2, T3> Event = new();
	}

	internal class EventInfo<T1, T2, T3, T4> : IEvent
	{
		public readonly UnityEvent<T1, T2, T3, T4> Event = new();
	}

	#endregion

	public class EventManager
	{
		private static readonly Lazy<EventManager> instance = new();

		public static EventManager Instance => instance.Value;

		private readonly Dictionary<string, IEvent> eventDict = new();

		private readonly Dictionary<string, Dictionary<string, int>> eventInfoDict = new();

		private const string ParamFlag0 = "0param";
		private const string ParamFlag1 = "1param";
		private const string ParamFlag2 = "2param";
		private const string ParamFlag3 = "3param";
		private const string ParamFlag4 = "4param";

		#region GetFullName

		private static string GetFullName(string eventName)
		{
			return $"{eventName}_{ParamFlag0}";
		}

		private static string GetFullName<T>(string eventName)
		{
			return $"{eventName}_{ParamFlag1}_{typeof(T)}";
		}

		private static string GetFullName<T1, T2>(string eventName)
		{
			return $"{eventName}_{ParamFlag2}_{typeof(T1)}_{typeof(T2)}";
		}

		private static string GetFullName<T1, T2, T3>(string eventName)
		{
			return $"{eventName}_{ParamFlag3}_{typeof(T1)}_{typeof(T2)}_{typeof(T3)}";
		}

		private static string GetFullName<T1, T2, T3, T4>(string eventName)
		{
			return $"{eventName}_{ParamFlag4}_{typeof(T1)}_{typeof(T2)}_{typeof(T3)}_{typeof(T4)}";
		}

		#endregion

		#region AddListener

		public void AddListener(string eventName, UnityAction action)
		{
			string eventFullName = GetFullName(eventName);

			if (!eventInfoDict.ContainsKey(eventName))
			{
				eventInfoDict.Add(eventName, new Dictionary<string, int>());
			}

			if (!eventDict.ContainsKey(eventFullName))
			{
				eventInfoDict[eventName].Add(eventFullName, 0);
				eventDict.Add(eventFullName, new EventInfo());
			}

			eventInfoDict[eventName][eventFullName]++;

			(eventDict[eventFullName] as EventInfo)!.Event.AddListener(action);
		}

		public void AddListener<T>(string eventName, UnityAction<T> action)
		{
			string eventFullName = GetFullName<T>(eventName);

			if (!eventInfoDict.ContainsKey(eventName))
			{
				eventInfoDict.Add(eventName, new Dictionary<string, int>());
			}

			if (!eventDict.ContainsKey(eventFullName))
			{
				eventInfoDict[eventName].Add(eventFullName, 0);
				eventDict.Add(eventFullName, new EventInfo<T>());
			}

			eventInfoDict[eventName][eventFullName]++;

			(eventDict[eventFullName] as EventInfo<T>)!.Event.AddListener(action);
		}

		public void AddListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
		{
			string eventFullName = GetFullName<T1, T2>(eventName);

			if (!eventInfoDict.ContainsKey(eventName))
			{
				eventInfoDict.Add(eventName, new Dictionary<string, int>());
			}

			if (!eventDict.ContainsKey(eventFullName))
			{
				eventInfoDict[eventName].Add(eventFullName, 0);
				eventDict.Add(eventFullName, new EventInfo<T1, T2>());
			}

			eventInfoDict[eventName][eventFullName]++;

			(eventDict[eventFullName] as EventInfo<T1, T2>)!.Event.AddListener(action);
		}

		public void AddListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
		{
			string eventFullName = GetFullName<T1, T2, T3>(eventName);

			if (!eventInfoDict.ContainsKey(eventName))
			{
				eventInfoDict.Add(eventName, new Dictionary<string, int>());
			}

			if (!eventDict.ContainsKey(eventFullName))
			{
				eventInfoDict[eventName].Add(eventFullName, 0);
				eventDict.Add(eventFullName, new EventInfo<T1, T2, T3>());
			}

			eventInfoDict[eventName][eventFullName]++;

			(eventDict[eventFullName] as EventInfo<T1, T2, T3>)!.Event.AddListener(action);
		}

		public void AddListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
		{
			string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

			if (!eventInfoDict.ContainsKey(eventName))
			{
				eventInfoDict.Add(eventName, new Dictionary<string, int>());
			}

			if (!eventDict.ContainsKey(eventFullName))
			{
				eventInfoDict[eventName].Add(eventFullName, 0);
				eventDict.Add(eventFullName, new EventInfo<T1, T2, T3, T4>());
			}

			eventInfoDict[eventName][eventFullName]++;

			(eventDict[eventFullName] as EventInfo<T1, T2, T3, T4>)!.Event.AddListener(action);
		}

		#endregion

		#region RemoveListener

		public void RemoveListener(string eventName, UnityAction action)
		{
			string eventFullName = GetFullName(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo)!.Event.RemoveListener(action);

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T>(string eventName, UnityAction<T> action)
		{
			string eventFullName = GetFullName<T>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T>)!.Event.RemoveListener(action);

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
		{
			string eventFullName = GetFullName<T1, T2>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T1, T2>)!.Event.RemoveListener(action);

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
		{
			string eventFullName = GetFullName<T1, T2, T3>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T1, T2, T3>)!.Event.RemoveListener(action);

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
		{
			string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T1, T2, T3, T4>)!.Event.RemoveListener(action);

			eventInfoDict[eventName][eventFullName]--;
		}

		#endregion

		#region Invoke

		public void Invoke(string eventName)
		{
			string eventFullName = GetFullName(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo)!.Event?.Invoke();
			}
		}

		public void Invoke<T>(string eventName, T t)
		{
			string eventFullName = GetFullName<T>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T>)!.Event?.Invoke(t);
			}
		}

		public void Invoke<T1, T2>(string eventName, T1 t1, T2 t2)
		{
			string eventFullName = GetFullName<T1, T2>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T1, T2>)!.Event?.Invoke(t1, t2);
			}
		}

		public void Invoke<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
		{
			string eventFullName = GetFullName<T1, T2, T3>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T1, T2, T3>)!.Event?.Invoke(t1, t2, t3);
			}
		}

		public void Invoke<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T1, T2, T3, T4>)!.Event?.Invoke(t1, t2, t3, t4);
			}
		}

		#endregion

		#region InvokePeriod

		/// <summary>
		/// Invoke the following events sequentially: <br/>
		/// {module}_Before{action} <br/>
		/// {module}_On{action} <br/>
		/// {module}_After{action} <br/>
		/// </summary>
		public void InvokePeriod(string module, string action)
		{
			Invoke($"{module}_Before{action}");
			Invoke($"{module}_On{action}");
			Invoke($"{module}_After{action}");
		}

		/// <summary>
		/// Invoke the following events sequentially: <br/>
		/// {module}_Before{action} <br/>
		/// {module}_On{action} <br/>
		/// {module}_After{action} <br/>
		/// </summary>
		public void InvokePeriod<T1>(string module, string action, T1 t1)
		{
			Invoke($"{module}_Before{action}", t1);
			Invoke($"{module}_On{action}", t1);
			Invoke($"{module}_After{action}", t1);
		}

		/// <summary>
		/// Invoke the following events sequentially: <br/>
		/// {module}_Before{action} <br/>
		/// {module}_On{action} <br/>
		/// {module}_After{action} <br/>
		/// </summary>
		public void InvokePeriod<T1, T2>(string module, string action, T1 t1, T2 t2)
		{
			Invoke($"{module}_Before{action}", t1, t2);
			Invoke($"{module}_On{action}", t1, t2);
			Invoke($"{module}_After{action}", t1, t2);
		}

		/// <summary>
		/// Invoke the following events sequentially: <br/>
		/// {module}_Before{action} <br/>
		/// {module}_On{action} <br/>
		/// {module}_After{action} <br/>
		/// </summary>
		public void InvokePeriod<T1, T2, T3>(string module, string action, T1 t1, T2 t2, T3 t3)
		{
			Invoke($"{module}_Before{action}", t1, t2, t3);
			Invoke($"{module}_On{action}", t1, t2, t3);
			Invoke($"{module}_After{action}", t1, t2, t3);
		}

		/// <summary>
		/// Invoke the following events sequentially: <br/>
		/// {module}_Before{action} <br/>
		/// {module}_On{action} <br/>
		/// {module}_After{action} <br/>
		/// </summary>
		public void InvokePeriod<T1, T2, T3, T4>(string module, string action, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			Invoke($"{module}_Before{action}", t1, t2, t3, t4);
			Invoke($"{module}_On{action}", t1, t2, t3, t4);
			Invoke($"{module}_After{action}", t1, t2, t3, t4);
		}

		#endregion

		#region AddVetoListener

		public void AddVetoListener(string eventName, UnityAction<VetoArg> action)
			=> AddListener(eventName, action);

		public void AddVetoListener<T>(string eventName, UnityAction<VetoArg, T> action)
			=> AddListener(eventName, action);

		public void AddVetoListener<T1, T2>(string eventName, UnityAction<VetoArg, T1, T2> action)
			=> AddListener(eventName, action);

		public void AddVetoListener<T1, T2, T3>(string eventName, UnityAction<VetoArg, T1, T2, T3> action)
			=> AddListener(eventName, action);

		#endregion

		#region RemoveVetoListener

		public void RemoveVetoListener(string eventName, UnityAction<VetoArg> action)
			=> RemoveListener(eventName, action);

		public void RemoveVetoListener<T>(string eventName, UnityAction<VetoArg, T> action)
			=> RemoveListener(eventName, action);

		public void RemoveVetoListener<T1, T2>(string eventName, UnityAction<VetoArg, T1, T2> action)
			=> RemoveListener(eventName, action);

		public void RemoveVetoListener<T1, T2, T3>(string eventName, UnityAction<VetoArg, T1, T2, T3> action)
			=> RemoveListener(eventName, action);

		#endregion

		#region InvokeVeto

		public bool InvokeVeto(string eventName, out IEnumerable<string> reasons)
		{
			VetoArg vetoArg = new();
			Invoke(eventName, vetoArg);
			reasons = vetoArg.Reasons;
			return vetoArg.CanExecute;
		}

		public bool InvokeVeto<T>(string eventName, T t, out IEnumerable<string> reasons)
		{
			VetoArg vetoArg = new();
			Invoke(eventName, vetoArg, t);
			reasons = vetoArg.Reasons;
			return vetoArg.CanExecute;
		}

		public bool InvokeVeto<T1, T2>(string eventName, T1 t1, T2 t2, out IEnumerable<string> reasons)
		{
			VetoArg vetoArg = new();
			Invoke(eventName, vetoArg, t1, t2);
			reasons = vetoArg.Reasons;
			return vetoArg.CanExecute;
		}

		public bool InvokeVeto<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3, out IEnumerable<string> reasons)
		{
			VetoArg vetoArg = new();
			Invoke(eventName, vetoArg, t1, t2, t3);
			reasons = vetoArg.Reasons;
			return vetoArg.CanExecute;
		}

		#endregion
	}
}