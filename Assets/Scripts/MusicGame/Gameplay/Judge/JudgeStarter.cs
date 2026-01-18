#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MusicGame.Gameplay.Judge
{
	public class JudgeStarter : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private GameObject[] inputProcessObjects = Array.Empty<GameObject>();

		// Private
		private readonly List<IInputProcessSystem> inputProcessSystems = new();

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			EnhancedTouchSupport.Enable();
			if (inputProcessSystems.Count == 0)
			{
				foreach (var go in inputProcessObjects)
				{
					if (go.TryGetComponent<IInputProcessSystem>(out var system))
					{
						inputProcessSystems.Add(system);
					}
				}
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			EnhancedTouchSupport.Disable();
		}

		void Update()
		{
			var touches = Touch.activeTouches;
			foreach (var system in inputProcessSystems) system.ProcessInput(touches);
		}
	}
}