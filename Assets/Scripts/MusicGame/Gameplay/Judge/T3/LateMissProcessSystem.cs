#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MusicGame.Gameplay.Judge.T3
{
	public class LateMissProcessSystem : T3MonoBehaviour, IInputProcessSystem, ISelfInstaller
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
		private IGameAudioPlayer music = default!;
		private ComboStorage comboStorage = default!;
		private JudgeStorage judgeStorage = default!;

		private int lastPotentialIndex = 0;

		// Constructor
		[Inject]
		private void Construct(
			IGameAudioPlayer music,
			ComboStorage comboStorage,
			JudgeStorage judgeStorage)
		{
			this.music = music;
			this.comboStorage = comboStorage;
			this.judgeStorage = judgeStorage;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			if (lastPotentialIndex >= comboStorage.Combos.Count) return;
			var current = music.ChartTime;
			var combo = comboStorage.Combos[lastPotentialIndex];
			while (combo.ExpectedTime + lateMissTime < current)
			{
				if (!judgeStorage.ContainsOrToContain(combo))
				{
					var judgeItem = combo.GetNewJudgeItem();
					if (judgeItem is IT3JudgeItem t3JudgeItem)
					{
						t3JudgeItem.ActualTime = combo.ExpectedTime + lateMissTime;
						t3JudgeItem.JudgedTouch = null;
						t3JudgeItem.JudgeResult = T3JudgeResult.LateMiss;
					}

					judgeStorage.AddJudgeItem(judgeItem);
				}

				lastPotentialIndex++;
				if (lastPotentialIndex >= touches.Count) break;
				combo = comboStorage.Combos[lastPotentialIndex];
			}
		}

		// Event Handlers
		private void UpdateComboInfo()
		{
			lastPotentialIndex = 0;
		}
	}
}