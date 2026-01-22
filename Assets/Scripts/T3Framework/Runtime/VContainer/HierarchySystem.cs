#nullable enable

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace T3Framework.Runtime.VContainer
{
	public abstract class HierarchySystem<T> : T3MonoBehaviour, ISelfInstaller where T : HierarchySystem<T>
	{
		// Serializable and Public
		// TODO: Use key to register component after VContainer supports it
		public virtual string? Key => null;

		public bool IsEnabled
		{
			get => gameObject.activeInHierarchy;
			set
			{
				gameObject.SetActive(value);
				if (value && !gameObject.activeInHierarchy)
				{
					Debug.LogWarning($"Fail to enable {GetType().Name}: parent is disabled");
				}
			}
		}

		// Constructor
		public virtual void SelfInstall(IContainerBuilder builder)
		{
			if (this is T self) builder.RegisterComponent(self).AsSelf();
		}
	}
}