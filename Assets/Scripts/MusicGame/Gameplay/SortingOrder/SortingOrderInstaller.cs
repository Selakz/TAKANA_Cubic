#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.SortingOrder
{
	public class SortingOrderInstaller : HierarchyInstaller
	{
		[SerializeField] private SequenceConfig sortingOrderPriority = default!;
		[SerializeField] private SequenceConfig sortingOrderConfig = default!;
		[SerializeField] private SpriteSortingOrderMapConfig sortingOrderMapConfig = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(sortingOrderConfig).Keyed("sortingOrder");
			builder.RegisterInstance(sortingOrderMapConfig);
			foreach (var type in sortingOrderMapConfig.Value.Keys)
			{
				builder.RegisterEntryPoint<SortingOrderSystem>()
					.WithParameter("targetType", type)
					.WithParameter("sortingOrderPriority", sortingOrderPriority.Priorities["basic"])
					.Keyed(type);
			}
		}
	}
}