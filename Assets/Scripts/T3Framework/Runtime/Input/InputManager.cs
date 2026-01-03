#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Preset.Event;
using T3Framework.Runtime.Event;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace T3Framework.Runtime.Input
{
	public class InputManager : NotifiableDataContainer<InputActionAsset?>, ISingletonMonoBehaviour<InputManager>
	{
		// Serializable and Public
		[Header("Optional")] [SerializeField] private InputActionAsset? inputActionAsset;

		public override InputActionAsset? InitialValue => inputActionAsset;

		public NotifiableProperty<bool> GlobalInputEnabled { get; } = new(true);

		public InputActionAsset? ActionAsset
		{
			get => Property.Value;
			set => Property.Value = value;
		}

		protected override IEventRegistrar[] AwakeRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<InputActionAsset?>(Property, (_, _) =>
			{
				var lastActionAsset = Property.LastValue;
				if (lastActionAsset != null)
				{
					foreach (var (index, actionSequence) in sequenceActionMap)
					{
						index.Unregister(lastActionAsset, actionSequence.Callback);
					}

					lastActionAsset.Disable();
				}

				if (ActionAsset != null)
				{
					foreach (var (index, actionSequence) in sequenceActionMap)
					{
						index.Register(ActionAsset, actionSequence.Callback);
					}

					ActionAsset.Enable();
				}
			})
		};

		// Private
		private readonly struct InputActionIndex : IEquatable<InputActionIndex>
		{
			private string ActionMapName { get; }
			private string ActionName { get; }
			private string SequenceGroup { get; }
			private InputActionPhase Phase { get; }

			public InputActionIndex
				(string actionMapName, string actionName, string sequenceGroup, InputActionPhase phase)
			{
				ActionMapName = actionMapName;
				ActionName = actionName;
				SequenceGroup = sequenceGroup;
				Phase = phase;
			}

			public bool Register(InputActionAsset? actionAsset, Action<InputAction.CallbackContext> callback)
			{
				if (actionAsset == null) return false;
				var actionMap = actionAsset.FindActionMap(ActionMapName);
				var action = actionMap?.FindAction(ActionName);
				if (action == null) return false;
				switch (Phase)
				{
					case InputActionPhase.Started:
						action.started += callback;
						break;
					case InputActionPhase.Performed:
						action.performed += callback;
						break;
					case InputActionPhase.Canceled:
						action.canceled += callback;
						break;
					default:
						return false;
				}

				return true;
			}

			public bool Unregister(InputActionAsset? actionAsset, Action<InputAction.CallbackContext> callback)
			{
				if (actionAsset == null) return false;
				var actionMap = actionAsset.FindActionMap(ActionMapName);
				var action = actionMap?.FindAction(ActionName, true);
				if (action == null) return false;
				switch (Phase)
				{
					case InputActionPhase.Started:
						action.started -= callback;
						break;
					case InputActionPhase.Performed:
						action.performed -= callback;
						break;
					case InputActionPhase.Canceled:
						action.canceled -= callback;
						break;
					default:
						return false;
				}

				return true;
			}

			public bool Equals(InputActionIndex other)
			{
				return ActionMapName == other.ActionMapName && ActionName == other.ActionName &&
				       SequenceGroup == other.SequenceGroup && Phase == other.Phase;
			}

			public override bool Equals(object? obj)
			{
				return obj is InputActionIndex other && Equals(other);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(ActionMapName, ActionName, SequenceGroup, Phase);
			}
		}

		private readonly struct InputActionSequence
		{
			public FunctionSequence Sequence { get; }

			public Action<InputAction.CallbackContext> Callback { get; }

			public InputActionSequence(FunctionSequence sequence, Action<InputAction.CallbackContext> callback)
			{
				Sequence = sequence;
				Callback = callback;
			}
		}

		private readonly Dictionary<InputActionIndex, InputActionSequence> sequenceActionMap = new();
		private readonly Dictionary<InputActionIndex, InputAction.CallbackContext> sequenceContextMap = new();

		// Defined Functions
		private InputAction.CallbackContext GetContext(InputActionIndex index) => sequenceContextMap[index];

		public void Register(string actionMapName, string actionName, string sequenceGroup, int priority,
			Func<InputAction.CallbackContext, bool> action, Action<int>? notify = null,
			InputActionPhase phase = InputActionPhase.Performed)
		{
			if (phase is InputActionPhase.Waiting or InputActionPhase.Disabled)
			{
				Debug.LogError($"Register with phase {phase} is not allowed.");
				return;
			}

			var index = new InputActionIndex(actionMapName, actionName, sequenceGroup, phase);
			if (!sequenceActionMap.ContainsKey(index))
			{
				Action<InputAction.CallbackContext> callback = context =>
				{
					sequenceContextMap[index] = context;
					sequenceActionMap[index].Sequence.Invoke();
				};
				InputActionSequence actionSequence = new(new(), callback);
				sequenceActionMap[index] = actionSequence;
				if (!index.Register(ActionAsset, callback))
				{
					Debug.LogWarning($"Register {actionMapName}.{actionName} failed in current input action asset.");
				}
			}

			sequenceActionMap[index].Sequence.Register(priority, () =>
			{
				if (!GlobalInputEnabled || IsFocusingOnTextField()) return true;
				var context = GetContext(index);
				var result = action.Invoke(context);
				return result;
			}, notify);
		}

		public void Unregister(string actionMapName, string actionName, string sequenceGroup, int priority,
			InputActionPhase phase = InputActionPhase.Performed)
		{
			var index = new InputActionIndex(actionMapName, actionName, sequenceGroup, phase);
			if (!sequenceActionMap.TryGetValue(index, out var actionSequence))
			{
				Debug.LogWarning($"Unregistering an unknown action {actionMapName}.{actionName}");
				return;
			}

			var res = actionSequence.Sequence.Unregister(priority);
			if (!res)
			{
				Debug.Log($"failed to unregister {actionMapName}.{actionName} of priority {priority}");
			}
		}

		private static bool IsFocusingOnTextField()
		{
			var selected = EventSystem.current.currentSelectedGameObject;
			if (selected == null) return false;
			InputField inputField = selected.GetComponent<InputField>();
			TMP_InputField inputField2 = selected.GetComponent<TMP_InputField>();
			return (inputField != null && inputField.isFocused) || (inputField2 != null && inputField2.isFocused);
		}

		// System Functions
		protected override void Awake()
		{
			base.Awake();
			ISingletonMonoBehaviour<InputManager>.Instance = this;
		}
	}
}