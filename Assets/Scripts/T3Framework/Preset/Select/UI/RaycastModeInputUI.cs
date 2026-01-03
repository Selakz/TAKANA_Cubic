#nullable enable

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

		private void UpdateMode()
		{
			RaycastMode.Value =
				polling
					? sole
						? PollingRaycastMode<T>.InstanceSole
						: PollingRaycastMode<T>.InstanceCtrl
					: sole
						? AllCastRaycastMode<T>.InstanceSole
						: AllCastRaycastMode<T>.InstanceCtrl;
		}
	}
}