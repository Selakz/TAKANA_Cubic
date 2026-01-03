#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerInstaller : HierarchyInstaller
	{
		[SerializeField] private SequencePriority colorPriority = default!;
		[SerializeField] private SequencePriority sortingOrderPriority = default!;
		[SerializeField] private PrefabObject contentPrefab = default!;
		[SerializeField] private Transform contentRoot = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<TrackLayerManageSystem>().AsSelf();
			builder.RegisterEntryPoint<TrackLayerViewSystem>()
				.WithParameter("layerColorPriority", colorPriority.Value)
				.WithParameter("layerSortingOrderPriority", sortingOrderPriority.Value);

			// ViewPool
			builder.Register<IViewPool<LayerComponent>, ViewPool<LayerComponent>>(Lifetime.Singleton)
				.WithParameter("prefab", contentPrefab)
				.WithParameter("defaultTransform", contentRoot)
				.Keyed("trackLayer");
			builder.RegisterEntryPoint<TrackLayerDisplaySystem>();
		}
	}
}