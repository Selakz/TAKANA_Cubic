#nullable enable

using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;

namespace MusicGame.Gameplay.Scoring.AutoScore
{
	public class AutoScoreInstaller : HierarchyInstaller
	{
		[SerializeField] private bool installComboFactory = true;

		public override void SelfInstall(IContainerBuilder builder)
		{
			if (installComboFactory) builder.Register<IComboFactory, T3ComboFactory>(Lifetime.Singleton);
		}
	}
}