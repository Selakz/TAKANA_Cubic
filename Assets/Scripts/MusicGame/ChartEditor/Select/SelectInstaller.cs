#nullable enable

using MusicGame.Gameplay.Chart;
using MusicGame.Models;
using T3Framework.Preset.Select;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;

namespace MusicGame.ChartEditor.Select
{
	public class SelectInstaller : HierarchyInstaller
	{
		[Header("Select Collider")] [SerializeField]
		private Transform colliderRoot = default!;

		[SerializeField] private PrefabObject colliderPrefab = default!;
		[SerializeField] private InspectorDictionary<T3Flag, TextureAlignInfo> textureAlignInfos = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<ChartSelectDataset>(Lifetime.Singleton);

			// SelectInputSystem
			builder.RegisterInstance(new NotifiableProperty<ISelectRaycastMode<ChartComponent>>
				(PollingRaycastMode<ChartComponent>.InstanceSole)).AsSelf();

			// SelectColliderPluginSystem
			builder.Register<IViewPool<ChartComponent>, ViewPool<ChartComponent>>(Lifetime.Singleton)
				.WithParameter("prefab", colliderPrefab)
				.WithParameter("defaultTransform", colliderRoot)
				.Keyed("select-collider");
			builder.RegisterInstance(textureAlignInfos.Value)
				.AsSelf()
				.Keyed("select-collider");
		}
	}
}