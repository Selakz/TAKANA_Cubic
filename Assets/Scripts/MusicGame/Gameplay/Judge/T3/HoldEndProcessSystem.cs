#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
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
	public class HoldEndProcessSystem : T3MonoBehaviour, IInputProcessSystem, ISelfInstaller
	{
		// Serializable and Public
		[Tooltip($"Should only contain {nameof(T3JudgeResult.CriticalJust)}")] [SerializeField]
		private T3JudgeConfig holdEndConfig = default!;

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
		private GameAudioPlayer music = default!;
		private TimeAligner aligner = default!;
		private ComboStorage comboStorage = default!;
		private JudgeStorage judgeStorage = default!;
		private StagePositionRetriever retriever = default!;

		private readonly Dictionary<IComboItem, HoldEndCombo> holdCombos = new();
		private readonly Dictionary<int, List<HoldEndCombo>> touchMap = new();

		// Constructor
		[Inject]
		private void Construct(
			GameAudioPlayer music,
			TimeAligner aligner,
			ComboStorage comboStorage,
			JudgeStorage judgeStorage,
			StagePositionRetriever retriever)
		{
			this.music = music;
			this.aligner = aligner;
			this.comboStorage = comboStorage;
			this.judgeStorage = judgeStorage;
			this.retriever = retriever;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public void ProcessInput(IReadOnlyList<Touch> touches)
		{
			// According to input system, the touch in touchMap must exist in following frames.
			foreach (var touch in touches)
			{
				if (!touchMap.TryGetValue(touch.touchId, out var endCombos)) continue;

				var chartTime = touch.phase == TouchPhase.Stationary
					? music.ChartTime
					: aligner.GetChartTime(touch.time);
				var position = retriever.GetPosition(touch.screenPosition);
				endCombos.RemoveAll(endCombo =>
				{
					var timeEnd = endCombo.FromComponent.Model.TimeMax;
					// 1. Can already be judged as complete
					if (holdEndConfig.IsInJudgeRange(timeEnd, chartTime, out var result))
					{
						judgeStorage.AddJudgeItemScheduled(new HoldEndJudgeItem(endCombo)
						{
							ActualTime = timeEnd,
							EndPosition = position,
							JudgedTouch = touch,
							JudgeResult = result
						}, timeEnd);
						return true;
					}
					// 2. Finger released but not completing the hold
					else if (touch.phase is TouchPhase.Canceled or TouchPhase.Ended)
					{
						judgeStorage.AddJudgeItem(new HoldEndJudgeItem(endCombo)
						{
							ActualTime = chartTime,
							EndPosition = position,
							JudgedTouch = touch,
							JudgeResult = T3JudgeResult.EarlyMiss
						});
						return true;
					}
					// 3. See if the touch leave the hold's judge range
					else
					{
						if (endCombo.FromComponent.Parent?.Model is not ITrack track) return true;
						var leftEdge = track.Movement.GetLeftPos(chartTime);
						var rightEdge = track.Movement.GetRightPos(chartTime);
						if (leftEdge > rightEdge) (leftEdge, rightEdge) = (rightEdge, leftEdge);
						if (leftEdge > position || rightEdge < position)
						{
							judgeStorage.AddJudgeItem(new HoldEndJudgeItem(endCombo)
							{
								ActualTime = chartTime,
								EndPosition = position,
								JudgedTouch = touch,
								JudgeResult = T3JudgeResult.EarlyMiss
							});
							return true;
						}
					}

					return false;
				});

				if (endCombos.Count == 0) touchMap.Remove(touch.touchId);
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
			if (judgeItem is HitJudgeItem hitJudgeItem)
			{
				if (hitJudgeItem.JudgeResult is T3JudgeResult.EarlyMiss or T3JudgeResult.LateMiss)
				{
					judgeStorage.AddJudgeItem(new HoldEndJudgeItem(endCombo)
					{
						ActualTime = hitJudgeItem.ActualTime,
						EndPosition = hitJudgeItem.TapPosition,
						JudgedTouch = hitJudgeItem.JudgedTouch,
						JudgeResult = T3JudgeResult.EarlyMiss
					});
				}
				else
				{
					if (hitJudgeItem.JudgedTouch is not { phase: TouchPhase.Began } touch)
					{
						Debug.LogError($"Hold's start judge is not judged by a touch of phase {TouchPhase.Began}");
						return;
					}

					var id = touch.touchId;
					if (!touchMap.ContainsKey(id)) touchMap.Add(id, new List<HoldEndCombo>(5));
					touchMap[id].Add(endCombo);
				}
			}
		}
	}
}