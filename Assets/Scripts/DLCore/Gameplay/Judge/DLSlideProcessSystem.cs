#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace DLCore.Gameplay.Judge
{
	public class DLSlideProcessSystem : T3MonoBehaviour, IInputProcessSystem<DLKeyInput>, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private T3JudgeConfig slideConfig = default!;
		[SerializeField] private DLKeyConfig keyConfig = default!;

		// Private
		[Inject] private IGameAudioPlayer music = default!;
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
				if (input.Phase == DLKeyPhase.Ended) continue;
				var chartTime = input.Phase == DLKeyPhase.Stationary
					? music.ChartTime
					: aligner.GetChartTime(input.InputTime);
				var keyColor = keyConfig.GetColor(input.Key);

				var startIndex = comboStorage.GetLowerBoundIndex(chartTime - endDistance);
				for (int i = startIndex;
				     i < comboStorage.Combos.Count && comboStorage.Combos[i].ExpectedTime < chartTime - startDistance;
				     i++)
				{
					var combo = comboStorage.Combos[i];
					if (combo is not DLHitCombo { NeedTap: false } hitCombo) continue;
					if (!DLKeyConfig.CanJudge(keyColor, hitCombo.Color)) continue;
					if (judgeStorage.ContainsOrToContain(hitCombo)) continue;
					judgeStorage.AddJudgeItemScheduled(new DLHitJudgeItem(hitCombo)
					{
						ActualTime = hitCombo.ExpectedTime,
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