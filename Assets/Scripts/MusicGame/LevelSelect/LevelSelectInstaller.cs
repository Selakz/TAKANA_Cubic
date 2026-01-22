#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime.ECS;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.LevelSelect
{
	public class LevelSelectInstaller : HierarchyInstaller
	{
		public override void SelfInstall(IContainerBuilder builder)
		{
			builder.Register<IDataset<LevelComponent<GameplayPreference>>,
				HashDataset<LevelComponent<GameplayPreference>>>(Lifetime.Singleton);
		}

		// System Functions
		void Awake()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 120;
			ScalableBufferManager.ResizeBuffers(0.25f, 0.25f);
		}
	}
}