#nullable enable

using DLCore.Models;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Stage;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace DLCore.Gameplay.Stage
{
	public class DLStageInstaller : HierarchyInstaller
	{
		[SerializeField] private InspectorDictionary<DLFlag, PrefabObject> prefabs = default!;
		[SerializeField] private Transform stageTransform = default!;
		[SerializeField] private GameplayStageSkinConfig stageSkinConfig = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			// ViewPool
			builder.Register<DLChartClassifier>(Lifetime.Singleton)
				.As<IClassifier<DLFlag>>()
				.Keyed("stage");
			builder.RegisterInstance(prefabs.Value)
				.AsSelf()
				.Keyed("stage");
			builder.RegisterInstance(stageTransform)
				.Keyed("stage");
			builder.Register<IViewPool<ChartComponent>, StageViewPool<DLFlag>>(Lifetime.Singleton)
				.Keyed("stage")
				.AsSelf();

			builder.RegisterNotifiableProperty(stageSkinConfig);
		}
	}
}