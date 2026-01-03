#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Speed
{
	public class SpeedInstaller : HierarchyInstaller
	{
		[SerializeField] private SpeedDataContainer speedContainer = default!;
		[SerializeField] private SequenceConfig notePositionConfig = default!;
		[SerializeField] private SequenceConfig noteSizeConfig = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(speedContainer);
			builder.RegisterInstance(speedContainer.Property);
			builder.RegisterEntryPoint<NoteSpeedSystem>()
				.WithParameter("positionPriority", notePositionConfig.Priorities["speed"]);
			builder.RegisterEntryPoint<HoldLengthSpeedSystem>()
				.WithParameter("lengthPriority", noteSizeConfig.Priorities["speed-height"]);
		}
	}
}