#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MusicGame.Gameplay.Judge.T3
{
	public class HoldEndProcessSystem : HierarchySystem<HoldEndProcessSystem>, IInputProcessSystem
	{
		// Serializable and Public
		[Tooltip("Seconds. Releasing or leaving the track within this time before the end is counted as complete.")]
		[SerializeField]
		private float holdGraceTime = 0.12f;

		[Tooltip("Seconds. The accumulated time a finger can be off the track before the hold is counted as a miss.")]
		[SerializeField]
		private float holdOutTime = 0.08f;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => comboStorage.OnComboReset += e,
				e => comboStorage.OnComboReset -= e,
				UpdateComboInfo),
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded -= e,
				OnHoldStartJudged),
		};

		// Private
		[Inject] private TimeAligner aligner = default!;
		[Inject] private ComboStorage comboStorage = default!;
		[Inject] private JudgeStorage judgeStorage = default!;
		[Inject] private StagePositionRetriever retriever = default!;

		private readonly Dictionary<IComboItem, HoldEndCombo> holdCombos = new();
		private readonly Dictionary<int, List<HoldEndState>> touchMap = new();

		private class HoldEndState
		{
			public HoldEndCombo Combo { get; set; }

			public bool IsOnTrack { get; set; }

			public T3Time OffTrackTime { get; set; }

			public HoldEndState(HoldEndCombo combo, bool isOnTrack, T3Time offTrackTime)
			{
				Combo = combo;
				IsOnTrack = isOnTrack;
				OffTrackTime = offTrackTime;
			}
		}

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			// According to input system, the touch in touchMap must exist in following frames.
			foreach (var touch in touches)
			{
				if (!touchMap.TryGetValue(touch.touchId, out var endCombos)) continue;

				var chartTime = touch.phase == TouchPhase.Stationary
					? aligner.GetCurrentChartTime()
					: aligner.GetChartTime(touch.time);
				var position = retriever.GetPosition(touch.screenPosition);
				var previousPosition = retriever.GetPosition(touch.screenPosition - touch.delta);
				var minPosition = Mathf.Min(position, previousPosition);
				var maxPosition = Mathf.Max(position, previousPosition);

				endCombos.RemoveAll(state =>
				{
					var timeEnd = state.Combo.ExpectedTime;
					var phaseEnded = touch.phase is TouchPhase.Canceled or TouchPhase.Ended;
					var onTrack = IsFingerOnTrack(state.Combo);
					// 1. Finger released
					if (phaseEnded)
					{
						// 1-1. Released near the end is counted as complete
						if (chartTime >= timeEnd - holdGraceTime) Complete(state);
						// 1-2. Released too early is counted as miss
						else AddEarlyMiss(state);
						return true;
					}

					// 2. Finger left the track
					if (!onTrack)
					{
						// 2-1. Left near the end is counted as complete
						if (chartTime >= timeEnd - holdGraceTime) Complete(state);
						else
						{
							// 2-2. Accumulate the time off the track
							if (state.IsOnTrack)
							{
								state.IsOnTrack = false;
								state.OffTrackTime = chartTime;
							}

							if (chartTime - state.OffTrackTime >= holdOutTime) AddEarlyMiss(state);
							else return false;
						}

						return true;
					}

					// 3. Holding on the track
					state.IsOnTrack = true;
					state.OffTrackTime = T3Time.MaxValue;
					if (chartTime < timeEnd - holdGraceTime) return false;
					Complete(state);
					return true;
				});

				if (endCombos.Count == 0) touchMap.Remove(touch.touchId);
				continue;

				bool IsFingerOnTrack(HoldEndCombo endCombo)
				{
					if (endCombo.FromComponent.Parent?.Model is not ITrack track) return true;
					var leftEdge = track.Movement.GetLeftPos(chartTime);
					var rightEdge = track.Movement.GetRightPos(chartTime);
					if (leftEdge > rightEdge) (leftEdge, rightEdge) = (rightEdge, leftEdge);
					return leftEdge - T3ComboFactory.ExtraRange <= maxPosition &&
					       rightEdge >= minPosition + T3ComboFactory.ExtraRange;
				}

				void Complete(HoldEndState state)
				{
					var endCombo = state.Combo;
					judgeStorage.AddJudgeItemScheduled(new HoldEndJudgeItem(endCombo)
					{
						ActualTime = endCombo.ExpectedTime,
						EndPosition = position,
						JudgedTouch = touch,
						JudgeResult = T3JudgeResult.CriticalJust
					}, endCombo.ExpectedTime);
				}

				void AddEarlyMiss(HoldEndState state)
				{
					judgeStorage.AddJudgeItem(new HoldEndJudgeItem(state.Combo)
					{
						ActualTime = chartTime,
						EndPosition = position,
						JudgedTouch = touch,
						JudgeResult = T3JudgeResult.EarlyMiss
					});
				}
			}
		}

		// Event Handlers
		private void UpdateComboInfo()
		{
			holdCombos.Clear();
			touchMap.Clear();
			for (int i = 0; i < comboStorage.Combos.Count; i++)
			{
				var combo = comboStorage.Combos[i];
				if (combo is HitCombo hitCombo && combo.FromComponent.Model is Hold)
				{
					for (int j = i + 1; j < comboStorage.Combos.Count; j++)
					{
						var endCombo = comboStorage.Combos[j];
						if (endCombo is HoldEndCombo holdEndCombo && endCombo.FromComponent == hitCombo.FromComponent)
						{
							holdCombos.Add(hitCombo, holdEndCombo);
						}
					}
				}
			}
		}

		private void OnHoldStartJudged(IJudgeItem judgeItem)
		{
			if (!holdCombos.TryGetValue(judgeItem.ComboItem, out var endCombo)) return;
			if (judgeItem is not HitJudgeItem hitJudgeItem) return;
			if (hitJudgeItem.JudgeResult is T3JudgeResult.EarlyMiss or T3JudgeResult.LateMiss)
			{
				judgeStorage.AddJudgeItem(new HoldEndJudgeItem(endCombo)
				{
					ActualTime = hitJudgeItem.ActualTime,
					EndPosition = hitJudgeItem.TapPosition,
					JudgedTouch = hitJudgeItem.JudgedTouch,
					JudgeResult = T3JudgeResult.EarlyMiss
				});
				return;
			}

			// A successful hold start that is not judged by a Began touch (e.g. autoplay)
			// relies on the end combo being judged on its own.
			if (hitJudgeItem.JudgedTouch is not { phase: TouchPhase.Began } touch) return;

			var id = touch.touchId;
			if (!touchMap.ContainsKey(id)) touchMap.Add(id, new List<HoldEndState>(5));
			touchMap[id].Add(new HoldEndState(endCombo, true, 0));
		}
	}
}