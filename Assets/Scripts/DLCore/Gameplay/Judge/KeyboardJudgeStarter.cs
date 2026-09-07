#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Judge;
using T3Framework.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace DLCore.Gameplay.Judge
{
	public class KeyboardJudgeStarter : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject[] inputProcessObjects = Array.Empty<GameObject>();

		// Private
		private readonly List<IInputProcessSystem<DLKeyInput>> inputProcessSystems = new();
		private readonly Queue<DLKeyInput> beganQueue = new();
		private readonly Queue<DLKeyInput> endedQueue = new();

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			if (inputProcessSystems.Count == 0)
			{
				foreach (var go in inputProcessObjects)
				{
					if (go.TryGetComponent<IInputProcessSystem<DLKeyInput>>(out var system))
					{
						inputProcessSystems.Add(system);
					}
				}
			}

			InputSystem.onEvent += OnInputEvent;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			InputSystem.onEvent -= OnInputEvent;
			beganQueue.Clear();
			endedQueue.Clear();
		}

		void Update()
		{
			if (Keyboard.current is not { } keyboard) return;
			var realtime = Time.realtimeSinceStartupAsDouble;
			List<DLKeyInput> inputs = new();
			HashSet<Key> beganKeys = new();

			while (beganQueue.Count > 0)
			{
				var input = beganQueue.Dequeue();
				beganKeys.Add(input.Key);
				inputs.Add(input);
			}

			foreach (var key in keyboard.allKeys)
			{
				if (key is null) continue;
				if (key.isPressed && !beganKeys.Contains(key.keyCode))
				{
					inputs.Add(new DLKeyInput(key.keyCode, DLKeyPhase.Stationary, realtime));
				}
			}

			while (endedQueue.Count > 0) inputs.Add(endedQueue.Dequeue());

			foreach (var system in inputProcessSystems) system.ProcessInput(inputs);
		}

		// Event Handlers
		private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
		{
			if (device is not Keyboard) return;
			if (!eventPtr.IsA<StateEvent>() && !eventPtr.IsA<DeltaStateEvent>()) return;
			var time = eventPtr.time;
			foreach (var control in eventPtr.EnumerateChangedControls(device: device))
			{
				if (control is not KeyControl keyControl) continue;
				var isDown = keyControl.ReadValueFromEvent(eventPtr) > 0.5f;
				if (isDown) beganQueue.Enqueue(new DLKeyInput(keyControl.keyCode, DLKeyPhase.Began, time));
				else endedQueue.Enqueue(new DLKeyInput(keyControl.keyCode, DLKeyPhase.Ended, time));
			}
		}
	}
}