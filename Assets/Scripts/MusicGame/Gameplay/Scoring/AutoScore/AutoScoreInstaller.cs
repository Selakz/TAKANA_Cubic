#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class AutoScoreInstaller : HierarchyInstaller
	{
		[SerializeField] private SequencePriority colorPriority = default!;
		[SerializeField] private SequencePriority positionPriority = default!;
		[SerializeField] private SequencePriority heightPriority = default!;

		[SerializeField] private PrefabObject hitEffectPrefab = default!;
		[SerializeField] private AudioSource hitSound = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<AutoScoreSystem>()
				.AsSelf();
			builder.RegisterEntryPoint<AutoScoreViewSystem>()
				.WithParameter("colorPriority", colorPriority.Value)
				.WithParameter("positionPriority", positionPriority.Value)
				.WithParameter("heightPriority", heightPriority.Value)
				.AsSelf();
			builder.RegisterEntryPoint<AutoScoreEffectSystem>()
				.WithParameter("hitEffectPrefab", hitEffectPrefab)
				.WithParameter("hitSound", hitSound);
		}
	}
}