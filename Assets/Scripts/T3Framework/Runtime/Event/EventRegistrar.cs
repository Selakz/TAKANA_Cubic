#nullable enable

using System;

namespace T3Framework.Runtime.Event
{
	public readonly struct EventRegistrar : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action action;

		public EventRegistrar(string eventName, Action action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct EventRegistrar<T> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<T> action;

		public EventRegistrar(string eventName, Action<T> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct EventRegistrar<T1, T2> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<T1, T2> action;

		public EventRegistrar(string eventName, Action<T1, T2> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct EventRegistrar<T1, T2, T3> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<T1, T2, T3> action;

		public EventRegistrar(string eventName, Action<T1, T2, T3> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct EventRegistrar<T1, T2, T3, T4> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<T1, T2, T3, T4> action;

		public EventRegistrar(string eventName, Action<T1, T2, T3, T4> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct VetoRegistrar : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<VetoArg> action;

		public VetoRegistrar(string eventName, Action<VetoArg> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddVetoListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct VetoRegistrar<T> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<VetoArg, T> action;

		public VetoRegistrar(string eventName, Action<VetoArg, T> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddVetoListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct VetoRegistrar<T1, T2> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<VetoArg, T1, T2> action;

		public VetoRegistrar(string eventName, Action<VetoArg, T1, T2> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddVetoListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}

	public readonly struct VetoRegistrar<T1, T2, T3> : IEventRegistrar
	{
		private readonly string eventName;
		private readonly Action<VetoArg, T1, T2, T3> action;

		public VetoRegistrar(string eventName, Action<VetoArg, T1, T2, T3> action)
		{
			this.eventName = eventName;
			this.action = action;
		}

		public void Register()
		{
			EventManager.Instance.AddVetoListener(eventName, action);
		}

		public void Unregister()
		{
			EventManager.Instance.RemoveListener(eventName, action);
		}
	}
}