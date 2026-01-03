#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Stage
{
	public class StageInstaller : HierarchyInstaller
	{
		[SerializeField] private InspectorDictionary<T3Flag, PrefabObject> prefabs = default!;
		[SerializeField] private Transform stageTransform = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			// ViewPool
			builder.Register<T3ChartClassifier>(Lifetime.Singleton)
				.As<IClassifier<T3Flag>>()
				.Keyed("stage");
			builder.RegisterInstance(prefabs.Value)
				.AsSelf()
				.Keyed("stage");
			builder.RegisterInstance(stageTransform)
				.Keyed("stage");
			builder.Register<IViewPool<ChartComponent>, StageViewPool>(Lifetime.Singleton)
				.Keyed("stage");
			// StageManager
			builder.RegisterEntryPoint<StageManager>()
				.AsSelf();

			// Trigger
			builder.RegisterEntryPoint<LevelStarter>();
		}
	}
}