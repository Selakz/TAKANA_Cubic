#nullable enable

using System.Collections.Generic;
using DLCore.Models;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DLCore.Gameplay.Judge
{
	public class DLHitProcessSystem : T3MonoBehaviour, IInputProcessSystem<DLKeyInput>, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private T3JudgeConfig defaultTapConfig = default!;
		[SerializeField] private T3JudgeConfig grayTapConfig = default!;
		[SerializeField] private DLKeyConfig keyConfig = default!;

		// Private
		[Inject] private TimeAligner aligner = default!;
		[Inject] private ComboStorage comboStorage = default!;
		[Inject] private JudgeStorage judgeStorage = default!;

		private T3Time startDistance = 0;
		private T3Time endDistance = 0;

		// Defined Functions
		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public void ProcessInput(IReadOnlyList<DLKeyInput> inputs)
		{
			foreach (var input in inputs)
			{
				if (input.Phase != DLKeyPhase.Began) continue;
				var chartTime = aligner.GetChartTime(input.InputTime);
				var keyColor = keyConfig.GetColor(input.Key);

				var startIndex = comboStorage.GetLowerBoundIndex(chartTime - endDistance);
				int nearestIndex = -1;
				int nearestDistance = int.MaxValue;
				T3JudgeResult result = T3JudgeResult.LateMiss;
				for (int i = startIndex;
				     i < comboStorage.Combos.Count && comboStorage.Combos[i].ExpectedTime < chartTime - startDistance;
				     i++)
				{
					var combo = comboStorage.Combos[i];
					if (combo is not DLHitCombo { NeedTap: true } hitCombo) continue;
					if (!DLKeyConfig.CanJudge(keyColor, hitCombo.Color)) continue;
					if (judgeStorage.ContainsOrToContain(hitCombo)) continue;
					var tapConfig = hitCombo.Color == ColorVariant.Gray ? grayTapConfig : defaultTapConfig;
					if (!tapConfig.IsInJudgeRange(combo.ExpectedTime, chartTime, out var candidateResult)) continue;

					var distance = Mathf.Abs(combo.ExpectedTime.Milli - chartTime.Milli);
					if (distance < nearestDistance)
					{
						nearestIndex = i;
						nearestDistance = distance;
						result = candidateResult;
					}
				}

				if (nearestIndex >= 0)
				{
					var combo = (DLHitCombo)comboStorage.Combos[nearestIndex];
					judgeStorage.AddJudgeItem(new DLHitJudgeItem(combo)
					{
						ActualTime = chartTime,
						JudgeResult = result
					});
				}
			}
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			startDistance = Mathf.Min(
				defaultTapConfig.ResultMap[T3JudgeResult.EarlyMiss].startTime,
				grayTapConfig.ResultMap[T3JudgeResult.CriticalJust].startTime); // Negative value
			endDistance = Mathf.Max(
				defaultTapConfig.ResultMap[T3JudgeResult.LateOk].endTime,
				grayTapConfig.ResultMap[T3JudgeResult.CriticalJust].endTime); // Positive value
		}
	}
}