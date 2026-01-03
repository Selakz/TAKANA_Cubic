#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.LaneBeam
{
	public class LaneBeamInstaller : HierarchyInstaller
	{
		[SerializeField] private PrefabObject laneBeamPrefab = default!;
		[SerializeField] private Transform laneBeamRoot = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<IViewPool<ChartComponent>, ViewPool<ChartComponent>>(Lifetime.Singleton)
				.WithParameter("prefab", laneBeamPrefab)
				.WithParameter("defaultTransform", laneBeamRoot)
				.Keyed("lane-beam");
			builder.RegisterEntryPoint<LaneBeamPluginSystem>();
		}
	}
}