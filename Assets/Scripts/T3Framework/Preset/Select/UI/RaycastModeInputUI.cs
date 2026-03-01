#nullable enable

using System.Collections.Generic;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Input;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace T3Framework.Preset.Select.UI
{
	public abstract class RaycastModeInputUI<T> : T3MonoBehaviour where T : IComponent
	{
		// Serializable and Public
		[SerializeField] private Toggle pollingToggle = default!;
		[SerializeField] private Toggle allCastToggle = default!;

		protected abstract NotifiableProperty<ISelectRaycastMode<T>> RaycastMode { get; }

		protected virtual IComparer<KeyValuePair<RaycastHit, T>>? PollingComparer => null;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ToggleRegistrar(pollingToggle, isOn =>
			{
				if (!isOn) return;
				polling = true;
				UpdateMode();
			}),
			new ToggleRegistrar(allCastToggle, isOn =>
			{
				if (!isOn) return;
				polling = false;
				UpdateMode();
			}),
			new InputRegistrar("General", "Ctrl", () =>
			{
				sole = false;
				UpdateMode();
			}, InputActionPhase.Started),
			new InputRegistrar("General", "Ctrl", () =>
			{
				sole = true;
				UpdateMode();
			}, InputActionPhase.Performed),
			new InputRegistrar("General", "Ctrl", () =>
			{
				sole = true;
				UpdateMode();
			}, InputActionPhase.Canceled),
		};

		// Private
		private bool polling = true;
		private bool sole = true;

		private PollingRaycastMode<T>? solePollingMode = null;
		private PollingRaycastMode<T>? ctrlPollingMode = null;

		private PollingRaycastMode<T> SolePollingMode =>
			solePollingMode ??= new PollingRaycastMode<T>(PollingComparer, false);

		private PollingRaycastMode<T> CtrlPollingMode =>
			ctrlPollingMode ??= new PollingRaycastMode<T>(PollingComparer, true);

		private void UpdateMode()
		{
			RaycastMode.Value =
				polling
					? sole
						? SolePollingMode
						: CtrlPollingMode
					: sole
						? AllCastRaycastMode<T>.InstanceSole
						: AllCastRaycastMode<T>.InstanceCtrl;
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			UpdateMode();
		}
	}
}