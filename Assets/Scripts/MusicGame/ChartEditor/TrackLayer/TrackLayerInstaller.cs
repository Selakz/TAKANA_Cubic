#nullable enable

using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.TrackLayer
{
	public class TrackLayerInstaller : HierarchyInstaller
	{
		[SerializeField] private PrefabObject contentPrefab = default!;
		[SerializeField] private Transform contentRoot = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			// ViewPool
			builder.Register<IViewPool<LayerComponent>, ViewPool<LayerComponent>>(Lifetime.Singleton)
				.WithParameter("prefab", contentPrefab)
				.WithParameter("defaultTransform", contentRoot)
				.Keyed("trackLayer");
		}
	}
}