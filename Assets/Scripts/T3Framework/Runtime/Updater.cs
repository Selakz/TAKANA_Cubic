#nullable enable

using UnityEngine;
using UnityEngine.Events;

namespace T3Framework.Runtime
{
	public class Updater : MonoBehaviour
	{
		public static event UnityAction? OnUpdate;

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		private static void UpdaterTrigger()
		{
			var go = new GameObject("Updater");
			go.AddComponent<Updater>();
			DontDestroyOnLoad(go);
		}

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