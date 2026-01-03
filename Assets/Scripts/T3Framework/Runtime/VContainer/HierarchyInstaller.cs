#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace T3Framework.Runtime.VContainer
{
	public class HierarchyInstaller : MonoBehaviour, ISelfInstaller, IInstaller
	{
		public void Install(IContainerBuilder builder)
		{
			SelfInstall(builder);
			foreach (var installer in GetDescendentInstallers())
			{
				installer.SelfInstall(builder);
			}
		}

		public virtual void SelfInstall(IContainerBuilder builder)
		{
		}

		public List<ISelfInstaller> GetDescendentInstallers()
		{
			List<ISelfInstaller> installers = new();
			foreach (Transform child in transform)
			{
				if (!child.gameObject.activeInHierarchy) continue;
				if (child.TryGetComponent<HierarchyLifetimeScope>(out _)) continue;
				if (child.TryGetComponent<HierarchyInstaller>(out var installer))
				{
					installers.Add(installer);
					installers.AddRange(installer.GetDescendentInstallers());
				}
				else if (child.TryGetComponent<ISelfInstaller>(out var selfInstaller))
				{
					installers.Add(selfInstaller);
				}
			}

			return installers;
		}

		[ContextMenu("Get Child Installers")]
		public void DebugDescendentInstallers()
		{
			var installers = GetDescendentInstallers();
			Debug.Log($"Descendent installers: {installers.Count}\n" +
			          $"{string.Join('\n', installers.Select(i => (i as MonoBehaviour)?.gameObject.name))}");
		}
	}
}