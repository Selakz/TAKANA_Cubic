#nullable enable

using System;
using T3Framework.Runtime.Event;

namespace T3Framework.Runtime.Log
{
	public static class T3Logger
	{
		private static string GetEventName(string module) => $"LogDispatcher_{module}";

		public static void Log(string module, string message)
		{
			EventManager.Instance.Invoke(GetEventName(module), message);
			EventManager.Instance.Invoke(GetEventName(module), message, T3LogType.Info);
		}

		public static void Log(string module, string message, Enum type)
		{
			EventManager.Instance.Invoke(GetEventName(module), message);
			EventManager.Instance.Invoke(GetEventName(module), message, type);
		}

		public static void AddListener(string module, Action<string> action)
		{
			EventManager.Instance.AddListener(GetEventName(module), action);
		}

		public static void AddListener(string module, Action<string, Enum> action)
		{
			EventManager.Instance.AddListener(GetEventName(module), action);
		}

		public static void RemoveListener(string module, Action<string> action)
		{
			EventManager.Instance.RemoveListener(GetEventName(module), action);
		}

		public static void RemoveListener(string module, Action<string, Enum> action)
		{
			EventManager.Instance.RemoveListener(GetEventName(module), action);
		}
	}
}