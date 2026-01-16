#nullable enable

using T3Framework.Preset.DataContainers;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Scoring
{
	public class ScoringInstaller : HierarchyInstaller
	{
		[SerializeField] private DoubleDataContainer scoreContainer = default!;
		[SerializeField] private IntegerDataContainer comboContainer = default!;
		[SerializeField] private IntegerDataContainer maxComboContainer = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(scoreContainer.Property)
				.Keyed("score");
			builder.RegisterInstance(comboContainer.Property)
				.Keyed("combo");
			builder.RegisterInstance(maxComboContainer.Property)
				.Keyed("maxCombo");
		}
	}
}