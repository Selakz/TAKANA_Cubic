#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MusicGame.Gameplay.Judge.T3
{
	public class HitProcessSystem : T3MonoBehaviour, IInputProcessSystem, ISelfInstaller
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
		private TimeAligner aligner = default!;
		private ComboStorage comboStorage = default!;
		private JudgeStorage judgeStorage = default!;
		private StagePositionRetriever retriever = default!;

		private readonly HashSet<HitCombo> pendingCombos = new(); // overlap combos can counteract a touch
		private HitCombo[] NearestCombos => nearestCombos ??= new HitCombo[overlapBufferCount];
		private HitCombo[]? nearestCombos;
		private T3Time startDistance = 0;
		private T3Time endDistance = 0;

		// Constructor
		[Inject]
		private void Construct(
			TimeAligner aligner,
			ComboStorage comboStorage,
			JudgeStorage judgeStorage,
			StagePositionRetriever retriever)
		{
			this.aligner = aligner;
			this.comboStorage = comboStorage;
			this.judgeStorage = judgeStorage;
			this.retriever = retriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			foreach (var touch in touches)
			{
				if (touch.phase != TouchPhase.Began) continue;
				var chartTime = aligner.GetChartTime(touch.startTime);
				var position = retriever.GetPosition(touch.startScreenPosition);

				var startIndex = comboStorage.GetLowerBoundIndex(chartTime + startDistance);
				int nearestCount = 0;
				bool isNearestPending = false;
				T3JudgeResult result = T3JudgeResult.LateMiss;
				for (int i = startIndex;
				     i < comboStorage.Combos.Count && comboStorage.Combos[i].ExpectedTime < chartTime + endDistance;
				     i++)
				{
					bool isPending = false;
					var combo = comboStorage.Combos[i];
					if (combo is not HitCombo { NeedTap: true } hitCombo) continue;
					// 1. If not in range, skip
					if (hitCombo.LeftEdge > position || hitCombo.RightEdge < position) continue;
					// 2. If judged and not in pending state, skip
					if (judgeStorage.ContainsOrToContain(hitCombo))
					{
						if (pendingCombos.Contains(hitCombo)) isPending = true;
						else continue;
					}

					// 3. Find the nearest tap and judge it
					if (nearestCount == 0)
					{
						if (!tapConfig.IsInJudgeRange(combo.ExpectedTime, chartTime, out result)) continue;
						NearestCombos[0] = hitCombo;
						nearestCount++;
						isNearestPending = isPending;
					}
					else if (nearestCount < overlapBufferCount)
					{
						var nearestDistance = Mathf.Abs(NearestCombos[0].ExpectedTime - chartTime);
						var currentDistance = Mathf.Abs(hitCombo.ExpectedTime - chartTime);
						const int tolerance = 2;
						const int pendingDebuff = 3; // Weird naming. Anyway it narrows pending notes' judge range
						if (!isPending && isNearestPending) nearestDistance *= pendingDebuff;
						if (currentDistance < nearestDistance - tolerance)
						{
							if (!tapConfig.IsInJudgeRange(combo.ExpectedTime, chartTime, out result)) continue;
							NearestCombos[0] = hitCombo;
							nearestCount = 1;
						}
						else if (Mathf.Abs(currentDistance - nearestDistance) <= tolerance)
						{
							NearestCombos[nearestCount] = hitCombo;
							nearestCount++;
						}

						isNearestPending = isPending;
					}
					else
					{
						Debug.LogError("overlap notes have exceeded the count of buffer");
					}
				}

				// If there is any combo, it must have a judge result.
				for (var i = 0; i < nearestCount; i++)
				{
					var combo = NearestCombos[i];
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
						if (nearestCount > 1) pendingCombos.Add(combo);
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