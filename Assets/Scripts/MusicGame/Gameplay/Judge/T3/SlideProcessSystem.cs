#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MusicGame.Gameplay.Judge.T3
{
	public class SlideProcessSystem : HierarchySystem<SlideProcessSystem>, IInputProcessSystem
	{
		// Serializable and Public
		[Tooltip($"Should only contain {nameof(T3JudgeResult.CriticalJust)}")] [SerializeField]
		private T3JudgeConfig slideConfig = default!;

		// Private
		[Inject] private TimeAligner aligner = default!;
		[Inject] private ComboStorage comboStorage = default!;
		[Inject] private JudgeStorage judgeStorage = default!;
		[Inject] private StagePositionRetriever retriever = default!;

		private T3Time startDistance = 0;
		private T3Time endDistance = 0;

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			foreach (var touch in touches)
			{
				var chartTime = touch.phase == TouchPhase.Stationary
					? aligner.GetCurrentChartTime()
					: aligner.GetChartTime(touch.time);
				var position = retriever.GetPosition(touch.screenPosition);
				var previousPosition = retriever.GetPosition(touch.screenPosition - touch.delta);
				var minPosition = Mathf.Min(position, previousPosition);
				var maxPosition = Mathf.Max(position, previousPosition);

				var startIndex = comboStorage.GetLowerBoundIndex(chartTime - endDistance);
				for (int i = startIndex;
				     i < comboStorage.Combos.Count && comboStorage.Combos[i].ExpectedTime < chartTime - startDistance;
				     i++)
				{
					var combo = comboStorage.Combos[i];
					if (combo is not HitCombo { NeedTap: false } hitCombo) continue;
					// 1. If not in range, skip
					if (hitCombo.LeftEdge > maxPosition || hitCombo.RightEdge < minPosition) continue;
					// 2. If judged, skip
					if (judgeStorage.ContainsOrToContain(hitCombo)) continue;
					// 3. Judge it scheduled
					judgeStorage.AddJudgeItemScheduled(new HitJudgeItem(hitCombo)
					{
						ActualTime = touch.phase == TouchPhase.Began ? chartTime : hitCombo.ExpectedTime,
						TapPosition = position,
						JudgedTouch = touch,
						JudgeResult = T3JudgeResult.CriticalJust
					}, hitCombo.ExpectedTime);
				}
			}
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			startDistance = slideConfig.ResultMap[T3JudgeResult.CriticalJust].startTime;
			endDistance = slideConfig.ResultMap[T3JudgeResult.CriticalJust].endTime;
		}
	}
}