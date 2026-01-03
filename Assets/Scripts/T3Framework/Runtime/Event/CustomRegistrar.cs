#nullable enable

using System;

namespace T3Framework.Runtime.Event
{
	public readonly struct CustomRegistrar : IEventRegistrar
	{
		private readonly Action? registerAction;
		private readonly Action? unregisterAction;

		public CustomRegistrar(Action registerAction, Action unregisterAction)
		{
			this.registerAction = registerAction;
			this.unregisterAction = unregisterAction;
		}

		public void Register()
		{
			registerAction?.Invoke();
		}

		public void Unregister()
		{
			unregisterAction?.Invoke();
		}

		public static CustomRegistrar Generic<T>(Action<T> registerAction, Action<T> unregisterAction, T doAction)
		{
			return new CustomRegistrar(RegisterAction, UnregisterAction);

			void RegisterAction() => registerAction.Invoke(doAction);
			void UnregisterAction() => unregisterAction.Invoke(doAction);
		}
	}
}