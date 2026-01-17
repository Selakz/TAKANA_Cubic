#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace MusicGame.Gameplay.Judge.T3
{
	public class SlideProcessSystem : T3MonoBehaviour, IInputProcessSystem, ISelfInstaller
	{
		// Serializable and Public
		[Tooltip($"Should only contain {nameof(T3JudgeResult.CriticalJust)}")] [SerializeField]
		private T3JudgeConfig slideConfig = default!;

		// Private
		private IGameAudioPlayer music = default!;
		private TimeAligner aligner = default!;
		private ComboStorage comboStorage = default!;
		private JudgeStorage judgeStorage = default!;
		private StagePositionRetriever retriever = default!;

		private T3Time startDistance = 0;
		private T3Time endDistance = 0;

		// Constructor
		[Inject]
		private void Construct(
			IGameAudioPlayer music,
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
			foreach (var touch in touches)
			{
				var chartTime = touch.phase == TouchPhase.Stationary
					? music.ChartTime
					: aligner.GetChartTime(touch.time);
				var position = retriever.GetPosition(touch.screenPosition);

				var startIndex = comboStorage.GetLowerBoundIndex(chartTime + startDistance);
				for (int i = startIndex;
				     i < comboStorage.Combos.Count && comboStorage.Combos[i].ExpectedTime < chartTime + endDistance;
				     i++)
				{
					var combo = comboStorage.Combos[i];
					if (combo is not HitCombo { NeedTap: false } hitCombo) continue;
					// 1. If not in range, skip
					if (hitCombo.LeftEdge > position || hitCombo.RightEdge < position) continue;
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