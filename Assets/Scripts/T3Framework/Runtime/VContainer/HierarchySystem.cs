#nullable enable

using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace T3Framework.Runtime.VContainer
{
	public abstract class HierarchySystem<T> : T3MonoBehaviour, ISelfInstaller where T : HierarchySystem<T>
	{
		// Serializable and Public
		// You know what? https://github.com/hadashiA/VContainer/pull/842
		public virtual string? Key => null;

		public virtual bool AsImplementedInterfaces => false;

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
			if (this is T self)
			{
				var registration = builder.RegisterComponent(self).AsSelf().Keyed(Key);
				if (AsImplementedInterfaces) registration.AsImplementedInterfaces();
			}
		}
	}
}