#nullable enable

using System;
using UnityEngine;

namespace T3Framework.Runtime
{
	public class Updater : MonoBehaviour, ISingletonMonoBehaviour<Updater>
	{
		public event Action? OnUpdate;
		public event Action? OnLateUpdate;
		public event Action? OnFixedUpdate;

		// System Functions
		private void Update()
		{
			OnUpdate?.Invoke();
		}

		private void LateUpdate()
		{
			OnLateUpdate?.Invoke();
		}

		private void FixedUpdate()
		{
			OnFixedUpdate?.Invoke();
		}

		private void OnDestroy()
		{
			OnUpdate = null;
			OnLateUpdate = null;
			OnFixedUpdate = null;
		}
	}
}