#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Basic.T3
{
	public class T3BasicInstaller : HierarchyInstaller
	{
		[SerializeField] private SequenceConfig notePositionConfig = default!;
		[SerializeField] private SequenceConfig noteSizeConfig = default!;
		[SerializeField] private SequenceConfig trackPositionConfig = default!;
		[SerializeField] private SequenceConfig trackSizeConfig = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterEntryPoint<T3NoteViewSystem>()
				.WithParameter("positionPriority", notePositionConfig.Priorities["basic"])
				.WithParameter("widthPriority", noteSizeConfig.Priorities["basic-width"]);
			builder.RegisterEntryPoint<T3HoldLengthSystem>()
				.WithParameter("lengthPriority", noteSizeConfig.Priorities["basic-height"]);
			builder.RegisterEntryPoint<T3TrackViewSystem>()
				.WithParameter("positionPriority", trackPositionConfig.Priorities["basic"])
				.WithParameter("widthPriority", trackSizeConfig.Priorities["basic"])
				.AsSelf();
		}
	}
}