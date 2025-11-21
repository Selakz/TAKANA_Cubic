#nullable enable

using System;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using UnityEngine.InputSystem;

namespace T3Framework.Runtime.Input
{
	public readonly struct InputRegistrar : IEventRegistrar
	{
		private const string GeneralSequenceName = "<@GeneralSequence>";
		private static int generalPriority = 0;

		private readonly string actionMapName;
		private readonly string actionName;
		private readonly string sequenceName;
		private readonly int priority;
		private readonly InputActionPhase phase;
		private readonly Func<InputAction.CallbackContext, bool> action;
		private readonly Action<int>? notify;

		public InputRegistrar(string actionMapName, string actionName, Action action,
			InputActionPhase phase = InputActionPhase.Performed)
			: this(actionMapName, actionName, GeneralSequenceName, generalPriority++,
				() =>
				{
					action.Invoke();
					return true;
				}, phase, delegate { })
		{
		}

		public InputRegistrar(string actionMapName, string actionName, Action<InputAction.CallbackContext> action,
			InputActionPhase phase = InputActionPhase.Performed)
			: this(actionMapName, actionName, GeneralSequenceName, generalPriority++,
				context =>
				{
					action.Invoke(context);
					return true;
				}, phase, delegate { })
		{
		}

		public InputRegistrar(string actionMapName, string actionName, string sequenceName, int priority,
			Func<bool> action, InputActionPhase phase = InputActionPhase.Performed, Action<int>? notify = null)
			: this(actionMapName, actionName, sequenceName, priority, _ => action.Invoke(), phase, notify)
		{
		}

		public InputRegistrar(string actionMapName, string actionName, string sequenceName, int priority,
			Func<InputAction.CallbackContext, bool> action,
			InputActionPhase phase = InputActionPhase.Performed,
			Action<int>? notify = null)
		{
			this.actionMapName = actionMapName;
			this.actionName = actionName;
			this.sequenceName = sequenceName;
			this.priority = priority;
			this.action = action;
			this.notify = notify;
			this.phase = phase;
		}

		public void Register()
		{
			ISingleton<NewInputManager>.Instance.Register
				(actionMapName, actionName, sequenceName, priority, action, notify, phase);
		}

		public void Unregister()
		{
			ISingleton<NewInputManager>.Instance.Unregister
				(actionMapName, actionName, sequenceName, priority);
		}
	}
}