using System;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// Modified from StarryFramework. I think it has improvement space but whatever.
namespace T3Framework.Runtime.Event
{
	#region EventBase

	internal interface IEvent
	{
	}

	internal class EventInfo : IEvent
	{
		public event Action Event;

		public void Invoke() => Event?.Invoke();
	}

	internal class EventInfo<T> : IEvent
	{
		public event Action<T> Event;

		public void Invoke(T arg) => Event?.Invoke(arg);
	}

	internal class EventInfo<T1, T2> : IEvent
	{
		public event Action<T1, T2> Event;

		public void Invoke(T1 arg1, T2 arg2) => Event?.Invoke(arg1, arg2);
	}

	internal class EventInfo<T1, T2, T3> : IEvent
	{
		public event Action<T1, T2, T3> Event;

		public void Invoke(T1 arg1, T2 arg2, T3 arg3) => Event?.Invoke(arg1, arg2, arg3);
	}

	internal class EventInfo<T1, T2, T3, T4> : IEvent
	{
		public event Action<T1, T2, T3, T4> Event;

		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4) => Event?.Invoke(arg1, arg2, arg3, arg4);
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

		public void AddListener(string eventName, Action action)
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

			(eventDict[eventFullName] as EventInfo)!.Event += action;
		}

		public void AddListener<T>(string eventName, Action<T> action)
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

			(eventDict[eventFullName] as EventInfo<T>)!.Event += action;
		}

		public void AddListener<T1, T2>(string eventName, Action<T1, T2> action)
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

			(eventDict[eventFullName] as EventInfo<T1, T2>)!.Event += action;
		}

		public void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
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

			(eventDict[eventFullName] as EventInfo<T1, T2, T3>)!.Event += action;
		}

		public void AddListener<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
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

			(eventDict[eventFullName] as EventInfo<T1, T2, T3, T4>)!.Event += action;
		}

		#endregion

		#region RemoveListener

		public void RemoveListener(string eventName, Action action)
		{
			string eventFullName = GetFullName(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo)!.Event -= action;

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T>(string eventName, Action<T> action)
		{
			string eventFullName = GetFullName<T>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T>)!.Event -= action;

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T1, T2>(string eventName, Action<T1, T2> action)
		{
			string eventFullName = GetFullName<T1, T2>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T1, T2>)!.Event -= action;

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
		{
			string eventFullName = GetFullName<T1, T2, T3>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T1, T2, T3>)!.Event -= action;

			eventInfoDict[eventName][eventFullName]--;
		}

		public void RemoveListener<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> action)
		{
			string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

			if (!eventDict.TryGetValue(eventFullName, out var value)) return;

			(value as EventInfo<T1, T2, T3, T4>)!.Event -= action;

			eventInfoDict[eventName][eventFullName]--;
		}

		#endregion

		#region Invoke

		public void Invoke(string eventName)
		{
			string eventFullName = GetFullName(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo)!.Invoke();
			}
		}

		public void Invoke<T>(string eventName, T t)
		{
			string eventFullName = GetFullName<T>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T>)!.Invoke(t);
			}
		}

		public void Invoke<T1, T2>(string eventName, T1 t1, T2 t2)
		{
			string eventFullName = GetFullName<T1, T2>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T1, T2>)!.Invoke(t1, t2);
			}
		}

		public void Invoke<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
		{
			string eventFullName = GetFullName<T1, T2, T3>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T1, T2, T3>)!.Invoke(t1, t2, t3);
			}
		}

		public void Invoke<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
		{
			string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

			if (eventDict.TryGetValue(eventFullName, out var value))
			{
				(value as EventInfo<T1, T2, T3, T4>)!.Invoke(t1, t2, t3, t4);
			}
		}

		#endregion

		#region AddVetoListener

		public void AddVetoListener(string eventName, Action<VetoArg> action)
			=> AddListener(eventName, action);

		public void AddVetoListener<T>(string eventName, Action<VetoArg, T> action)
			=> AddListener(eventName, action);

		public void AddVetoListener<T1, T2>(string eventName, Action<VetoArg, T1, T2> action)
			=> AddListener(eventName, action);

		public void AddVetoListener<T1, T2, T3>(string eventName, Action<VetoArg, T1, T2, T3> action)
			=> AddListener(eventName, action);

		#endregion

		#region RemoveVetoListener

		public void RemoveVetoListener(string eventName, Action<VetoArg> action)
			=> RemoveListener(eventName, action);

		public void RemoveVetoListener<T>(string eventName, Action<VetoArg, T> action)
			=> RemoveListener(eventName, action);

		public void RemoveVetoListener<T1, T2>(string eventName, Action<VetoArg, T1, T2> action)
			=> RemoveListener(eventName, action);

		public void RemoveVetoListener<T1, T2, T3>(string eventName, Action<VetoArg, T1, T2, T3> action)
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