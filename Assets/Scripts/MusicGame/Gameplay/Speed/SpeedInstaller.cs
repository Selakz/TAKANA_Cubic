#nullable enable

using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Speed
{
	public class SpeedInstaller : HierarchyInstaller
	{
		[SerializeField] private SpeedDataContainer speedContainer = default!;

		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.RegisterInstance(speedContainer);
			builder.RegisterInstance(speedContainer.Property);
		}
	}
}