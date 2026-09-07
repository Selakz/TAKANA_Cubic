#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DLCore.Gameplay.Judge
{
	public class DLLateMissProcessSystem : T3MonoBehaviour, IInputProcessSystem<DLKeyInput>, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private int lateMissTime;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => comboStorage.OnComboReset += e,
				e => comboStorage.OnComboReset -= e,
				UpdateComboInfo),
		};

		// Private
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private ComboStorage comboStorage = default!;
		[Inject] private JudgeStorage judgeStorage = default!;

		private int lastPotentialIndex = 0;

		// Defined Functions
		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public void ProcessInput(IReadOnlyList<DLKeyInput> inputs)
		{
			if (lastPotentialIndex >= comboStorage.Combos.Count) return;
			var current = music.ChartTime;
			var combo = comboStorage.Combos[lastPotentialIndex];
			while (combo.ExpectedTime + lateMissTime < current)
			{
				if (!judgeStorage.ContainsOrToContain(combo))
				{
					var judgeItem = combo.GetNewJudgeItem();
					if (judgeItem is IDLJudgeItem dlJudgeItem)
					{
						dlJudgeItem.ActualTime = combo.ExpectedTime + lateMissTime;
						dlJudgeItem.JudgeResult = T3JudgeResult.LateMiss;
					}

					judgeStorage.AddJudgeItem(judgeItem);
				}

				lastPotentialIndex++;
				if (lastPotentialIndex >= comboStorage.Combos.Count) break;
				combo = comboStorage.Combos[lastPotentialIndex];
			}
		}

		// Event Handlers
		private void UpdateComboInfo() => lastPotentialIndex = 0;
	}
}
