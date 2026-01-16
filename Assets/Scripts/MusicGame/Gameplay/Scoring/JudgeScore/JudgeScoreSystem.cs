#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.JudgeScore
{
	public class JudgeScoreSystem : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<T3JudgeResult, float> scoreRates = default!;
		[SerializeField] private List<T3JudgeResult> offComboResults = default!;

		// Private
		private NotifiableProperty<double> score = default!;
		private NotifiableProperty<int> combo = default!;
		private NotifiableProperty<int> maxCombo = default!;
		private ComboStorage comboStorage = default!;
		private JudgeStorage judgeStorage = default!;

		private double averageScore;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => comboStorage.OnComboReset += e,
				e => comboStorage.OnComboReset -= e,
				UpdateInfo),
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded -= e,
				UpdateScoreAndCombo),
			CustomRegistrar.Generic<Action>(
				e => judgeStorage.OnJudgeItemCleared += e,
				e => judgeStorage.OnJudgeItemCleared -= e,
				ClearScoreAndCombo),
		};

		// Static
		private const double MaxScore = 1_000_000;

		// Constructor
		[Inject]
		private void Construct(
			[Key("score")] NotifiableProperty<double> score,
			[Key("combo")] NotifiableProperty<int> combo,
			[Key("maxCombo")] NotifiableProperty<int> maxCombo,
			ComboStorage comboStorage,
			JudgeStorage judgeStorage)
		{
			this.score = score;
			this.combo = combo;
			this.maxCombo = maxCombo;
			this.comboStorage = comboStorage;
			this.judgeStorage = judgeStorage;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void UpdateInfo()
		{
			averageScore = MaxScore / comboStorage.Combos.Count;
		}

		private void UpdateScoreAndCombo(IJudgeItem judgeItem)
		{
			if (judgeItem is not IT3JudgeItem t3JudgeItem) return;
			double scoreRate = scoreRates.Value.GetValueOrDefault(t3JudgeItem.JudgeResult, 0);
			score.Value += averageScore * scoreRate;
			if (offComboResults.Contains(t3JudgeItem.JudgeResult)) combo.Value = 0;
			else combo.Value++;
			maxCombo.Value = Mathf.Max(maxCombo.Value, combo.Value);
		}

		private void ClearScoreAndCombo()
		{
			score.Value = 0;
			combo.Value = 0;
		}
	}
}