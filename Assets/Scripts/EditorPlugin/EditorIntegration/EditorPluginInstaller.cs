#nullable enable

using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace EditorPlugin.EditorIntegration
{
	public class EditorPluginInstaller : HierarchyInstaller
	{
		[SerializeField] private ViewPoolInstaller pluginPoolInstaller;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<IDataset<PluginComponent>, HashDataset<PluginComponent>>(Lifetime.Singleton);
			builder.RegisterViewPool<PluginComponent>(pluginPoolInstaller);
		}
	}
}