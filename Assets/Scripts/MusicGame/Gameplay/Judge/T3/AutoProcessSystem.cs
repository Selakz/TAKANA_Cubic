#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine.InputSystem.EnhancedTouch;
using VContainer;

namespace MusicGame.Gameplay.Judge.T3
{
	public class AutoProcessSystem : HierarchySystem<AutoProcessSystem>, IInputProcessSystem
	{
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

		private int nextIndex = 0;

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			if (nextIndex >= comboStorage.Combos.Count) return;
			var current = music.ChartTime;
			var combo = comboStorage.Combos[nextIndex];
			while (combo.ExpectedTime <= current)
			{
				if (!judgeStorage.ContainsOrToContain(combo))
				{
					var judgeItem = combo.GetNewJudgeItem();
					if (judgeItem is IT3JudgeItem t3JudgeItem)
					{
						t3JudgeItem.ActualTime = combo.ExpectedTime;
						t3JudgeItem.JudgedTouch = null;
						t3JudgeItem.JudgeResult = T3JudgeResult.CriticalJust;
					}

					judgeStorage.AddJudgeItem(judgeItem);
				}

				nextIndex++;
				if (nextIndex >= comboStorage.Combos.Count) break;
				combo = comboStorage.Combos[nextIndex];
			}
		}

		// Event Handlers
		private void UpdateComboInfo()
		{
			nextIndex = 0;
		}
	}
}