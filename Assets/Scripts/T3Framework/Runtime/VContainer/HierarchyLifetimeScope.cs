#nullable enable

using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace T3Framework.Runtime.VContainer
{
	public class HierarchyLifetimeScope : LifetimeScope
	{
		[SerializeField] private List<HierarchyInstaller> installers = new();

		protected override void Configure(IContainerBuilder builder) => installers.ForEach(i => i.Install(builder));
	}
}