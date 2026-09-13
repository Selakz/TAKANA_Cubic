#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MusicGame.Gameplay.Judge.T3
{
	public class HitProcessSystem : HierarchySystem<HitProcessSystem>, IInputProcessSystem
	{
		// Serializable and Public
		[Tooltip($"Should contain all enums except {nameof(T3JudgeResult.LateMiss)}")] [SerializeField]
		private T3JudgeConfig tapConfig = default!;

		[SerializeField] private int overlapBufferCount = 10;

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
		[Inject] private StagePositionRetriever retriever = default!;

		private readonly HashSet<HitCombo> pendingCombos = new(); // overlap combos can counteract a touch
		private HitCombo[] EarliestCombos => earliestCombos ??= new HitCombo[overlapBufferCount];
		private HitCombo[]? earliestCombos;
		private T3Time startDistance = 0;
		private T3Time endDistance = 0;

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			foreach (var touch in touches)
			{
				if (touch.phase != TouchPhase.Began) continue;
				var chartTime = aligner.GetChartTime(touch.startTime);
				var position = retriever.GetPosition(touch.startScreenPosition);

				var startIndex = comboStorage.GetLowerBoundIndex(chartTime - endDistance);
				int earliestNoteCount = 0;
				bool isEarliestNotePending = false;
				T3JudgeResult result = T3JudgeResult.LateMiss;
				for (int i = startIndex;
				     i < comboStorage.Combos.Count && comboStorage.Combos[i].ExpectedTime < chartTime - startDistance;
				     i++)
				{
					bool isCurrentNotePending = false;
					var combo = comboStorage.Combos[i];
					if (combo is not HitCombo { NeedTap: true } hitCombo) continue;
					// 1. If not in range, skip
					if (hitCombo.LeftEdge > position || hitCombo.RightEdge < position) continue;

					// 2. If judged and not in pending state, skip
					if (judgeStorage.ContainsOrToContain(hitCombo))
					{
						if (pendingCombos.Contains(hitCombo)) isCurrentNotePending = true;
						else continue;
					}

					// 3. Find the first tap which can be judged and judge it. Combos are scanned in ascending time order.
					if (earliestNoteCount == 0)
					{
						if (!tapConfig.IsInJudgeRange(combo.ExpectedTime, chartTime, out result)) continue;
						EarliestCombos[0] = hitCombo;
						earliestNoteCount++;
						isEarliestNotePending = isCurrentNotePending;
					}
					// 4. Find taps which are at the "same" time with the earliest tap
					else if (earliestNoteCount < overlapBufferCount)
					{
						var baseTime = EarliestCombos[0].ExpectedTime;
						const int tolerance = 2;
						const int pendingDebuff = 3; // A fresh combo can take over a pending one even if it is a bit later
						if (!isCurrentNotePending && isEarliestNotePending)
						{
							if (hitCombo.ExpectedTime - baseTime <= tolerance * pendingDebuff)
							{
								if (!tapConfig.IsInJudgeRange(combo.ExpectedTime, chartTime, out result)) continue;
								EarliestCombos[0] = hitCombo;
								earliestNoteCount = 1;
								isEarliestNotePending = false;
							}
						}
						else if (hitCombo.ExpectedTime - baseTime <= tolerance)
						{
							EarliestCombos[earliestNoteCount] = hitCombo;
							earliestNoteCount++;
							if (isEarliestNotePending) isEarliestNotePending = isCurrentNotePending;
						}
					}
					else
					{
						Debug.LogError("overlap notes have exceeded the count of buffer");
					}
				}

				// If there is any combo, it must have a judge result.
				for (var i = 0; i < earliestNoteCount; i++)
				{
					var combo = EarliestCombos[i];
					if (pendingCombos.Contains(combo))
					{
						pendingCombos.Remove(combo);
					}
					else
					{
						judgeStorage.AddJudgeItem(new HitJudgeItem(combo)
						{
							ActualTime = chartTime,
							TapPosition = position,
							JudgedTouch = touch,
							JudgeResult = result
						});
						if (earliestNoteCount > 1) pendingCombos.Add(combo);
					}
				}
			}
		}

		// Event Handlers
		private void UpdateComboInfo()
		{
			pendingCombos.Clear();
		}

		// System Functions
		protected override void OnEnable()
		{
			base.OnEnable();
			startDistance = tapConfig.ResultMap[T3JudgeResult.EarlyMiss].startTime;
			endDistance = tapConfig.ResultMap[T3JudgeResult.LateOk].endTime;
		}
	}
}