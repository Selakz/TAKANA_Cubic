#nullable enable

using System;
using UnityEngine;

namespace T3Framework.Runtime.Event.UI
{
	public class LifetimeListener : MonoBehaviour
	{
		// Serializable and Public
		public event Action<bool>? OnEnableStateChanged;

		// System Functions
		void OnEnable() => OnEnableStateChanged?.Invoke(true);

		void OnDisable() => OnEnableStateChanged?.Invoke(false);
	}

	public class LifetimeRegistrar : IEventRegistrar
	{
		private readonly LifetimeListener listener;
		private readonly Action<bool> action;
		private readonly bool invokeOnRegister;

		public LifetimeRegistrar(LifetimeListener listener, Action<bool> action, bool invokeOnRegister = true)
		{
			this.listener = listener;
			this.action = action;
			this.invokeOnRegister = invokeOnRegister;
		}

		public void Register()
		{
			listener.OnEnableStateChanged += action;
			if (invokeOnRegister) action.Invoke(listener.gameObject.activeInHierarchy);
		}

		public void Unregister() => listener.OnEnableStateChanged -= action;
	}
}