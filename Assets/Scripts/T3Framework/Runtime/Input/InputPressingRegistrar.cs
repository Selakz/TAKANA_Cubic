#nullable enable

using System;
using T3Framework.Runtime.Event;
using UnityEngine.InputSystem;

namespace T3Framework.Runtime.Input
{
	/// <summary>
	/// Check if an input action is pressing with no priority.
	/// </summary>
	public readonly struct InputPressingRegistrar : IEventRegistrar
	{
		private readonly Action<bool> setter;
		private readonly InputRegistrar startedRegistrar;
		private readonly InputRegistrar performedRegistrar;
		private readonly InputRegistrar canceledRegistrar;

		// No Priority
		public InputPressingRegistrar(string actionMapName, string actionName, Action<bool> setter)
		{
			this.setter = setter;
			startedRegistrar = new InputRegistrar(actionMapName, actionName, () => setter.Invoke(true),
				InputActionPhase.Started);
			performedRegistrar = new InputRegistrar(actionMapName, actionName, () => setter.Invoke(false),
				InputActionPhase.Performed);
			canceledRegistrar = new InputRegistrar(actionMapName, actionName, () => setter.Invoke(false),
				InputActionPhase.Canceled);
		}

		public void Register()
		{
			startedRegistrar.Register();
			performedRegistrar.Register();
			canceledRegistrar.Register();
		}

		public void Unregister()
		{
			startedRegistrar.Unregister();
			performedRegistrar.Unregister();
			canceledRegistrar.Unregister();
			setter.Invoke(false);
		}
	}
}