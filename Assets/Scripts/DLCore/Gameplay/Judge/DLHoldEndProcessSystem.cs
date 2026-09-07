#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using DLCore.Models;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Models.Note;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DLCore.Gameplay.Judge
{
	public class DLHoldEndProcessSystem : T3MonoBehaviour, IInputProcessSystem<DLKeyInput>, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private T3JudgeConfig holdEndConfig = default!;
		[SerializeField] private DLKeyConfig keyConfig = default!;

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
		[Inject] private IGameAudioPlayer music = default!;
		[Inject] private ComboStorage comboStorage = default!;
		[Inject] private JudgeStorage judgeStorage = default!;

		private readonly Dictionary<IComboItem, DLHoldEndCombo> holdCombos = new();
		private readonly HashSet<DLHoldEndCombo> activeHoldEnds = new();

		// Defined Functions
		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		public void ProcessInput(IReadOnlyList<DLKeyInput> inputs)
		{
			if (activeHoldEnds.Count == 0) return;
			var chartTime = music.ChartTime;
			HashSet<ColorVariant> heldColors = new();
			foreach (var input in inputs)
			{
				if (input.Phase is DLKeyPhase.Began or DLKeyPhase.Stationary)
					heldColors.Add(keyConfig.GetColor(input.Key));
			}

			activeHoldEnds.RemoveWhere(endCombo =>
			{
				if (heldColors.Any(color => DLKeyConfig.CanJudge(color, endCombo.Color)))
				{
					if (holdEndConfig.IsInJudgeRange(endCombo.ExpectedTime, chartTime, out var result))
					{
						judgeStorage.AddJudgeItemScheduled(new DLHoldEndJudgeItem(endCombo)
						{
							ActualTime = endCombo.ExpectedTime,
							JudgeResult = result
						}, endCombo.ExpectedTime);
						return true;
					}

					return false;
				}

				judgeStorage.AddJudgeItem(new DLHoldEndJudgeItem(endCombo)
				{
					ActualTime = chartTime,
					JudgeResult = T3JudgeResult.EarlyMiss
				});
				return true;
			});
		}

		// Event Handlers
		private void UpdateComboInfo()
		{
			activeHoldEnds.Clear();
			holdCombos.Clear();
			for (int i = 0; i < comboStorage.Combos.Count; i++)
			{
				var combo = comboStorage.Combos[i];
				if (combo is DLHitCombo { FromComponent: { Model: Hold } } hitCombo)
				{
					for (int j = i + 1; j < comboStorage.Combos.Count; j++)
					{
						var endCombo = comboStorage.Combos[j];
						if (endCombo is DLHoldEndCombo holdEndCombo &&
						    holdEndCombo.FromComponent == hitCombo.FromComponent)
						{
							holdCombos.Add(hitCombo, holdEndCombo);
						}
					}
				}
			}
		}

		private void OnHoldStartJudged(IJudgeItem judgeItem)
		{
			if (judgeItem is not DLHitJudgeItem hitJudgeItem) return;
			if (!holdCombos.TryGetValue(hitJudgeItem.ComboItem, out var endCombo)) return;
			if (hitJudgeItem.JudgeResult is T3JudgeResult.EarlyMiss or T3JudgeResult.LateMiss)
			{
				judgeStorage.AddJudgeItem(new DLHoldEndJudgeItem(endCombo)
				{
					ActualTime = hitJudgeItem.ActualTime,
					JudgeResult = T3JudgeResult.EarlyMiss
				});
			}
			else if (holdEndConfig.IsInJudgeRange(endCombo.ExpectedTime, hitJudgeItem.ActualTime, out var result))
			{
				judgeStorage.AddJudgeItemScheduled(new DLHoldEndJudgeItem(endCombo)
				{
					ActualTime = endCombo.ExpectedTime,
					JudgeResult = result
				}, endCombo.ExpectedTime);
			}
			else
			{
				activeHoldEnds.Add(endCombo);
			}
		}
	}
}