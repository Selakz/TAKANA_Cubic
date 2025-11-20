#nullable enable

using System;
using T3Framework.Static;
using UnityEngine;
using Object = UnityEngine.Object;

namespace T3Framework.Runtime
{
	public static class SingletonMonoBehaviourRegistrar
	{
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void MonoBehaviourFactory()
		{
			SingletonFactories.Register("MonoBehaviour", type =>
			{
				if (!type.IsSubclassOf(typeof(MonoBehaviour)))
				{
					throw new InvalidOperationException($"Type {type.Name} is not a subclass of MonoBehaviour.");
				}

				if (type.IsAbstract || type.IsGenericType)
				{
					throw new InvalidOperationException($"Type {type.Name} should be a concrete class.");
				}

				Debug.Log($"Instantiating {type.Name}...");
				var go = new GameObject(type.Name);
				var instance = go.AddComponent(type);
				Object.DontDestroyOnLoad(go);
				return instance;
			});
		}
	}

	[SingletonFactory("MonoBehaviour")]
	public interface ISingletonMonoBehaviour<out T> : ISingleton<T> where T : ISingletonMonoBehaviour<T>
	{
	}
}