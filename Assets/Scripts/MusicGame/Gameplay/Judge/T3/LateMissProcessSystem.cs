#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MusicGame.Gameplay.Judge.T3
{
	public class LateMissProcessSystem : HierarchySystem<LateMissProcessSystem>, IInputProcessSystem
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
		[Inject] private TimeAligner aligner = default!;
		[Inject] private ComboStorage comboStorage = default!;
		[Inject] private JudgeStorage judgeStorage = default!;

		private int lastPotentialIndex = 0;

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			if (lastPotentialIndex >= comboStorage.Combos.Count) return;
			var current = aligner.GetCurrentChartTime();
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
				if (lastPotentialIndex >= comboStorage.Combos.Count) break;
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