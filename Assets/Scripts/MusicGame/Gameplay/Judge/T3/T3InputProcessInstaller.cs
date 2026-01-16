#nullable enable

using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Judge.T3
{
	public class T3InputProcessInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<StagePositionRetriever>(Lifetime.Singleton)
				.WithParameter("gamePlane", new Plane(Vector3.forward, Vector3.zero));
		}
	}
}