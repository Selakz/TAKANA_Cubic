#nullable enable

using UnityEngine;
using UnityEngine.Events;

namespace T3Framework.Runtime
{
	public class Updater : MonoBehaviour, ISingletonMonoBehaviour<Updater>
	{
		public event UnityAction? OnUpdate;

		// System Functions
		private void Update()
		{
			OnUpdate?.Invoke();
		}

		private void OnDestroy()
		{
			OnUpdate = null;
		}
	}
}