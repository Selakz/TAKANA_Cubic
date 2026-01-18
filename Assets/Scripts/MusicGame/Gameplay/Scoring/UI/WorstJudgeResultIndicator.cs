#nullable enable

using System;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.UI
{
	public class WorstJudgeResultIndicator : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private T3JudgeResultConfig config = default!;
		[SerializeField] private T3JudgeResult initialResult;
		[SerializeField] private Graphic targetGraphic = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => judgeStorage.OnJudgeItemCleared += e,
				e => judgeStorage.OnJudgeItemCleared -= e,
				() =>
				{
					worstResult = initialResult;
					targetGraphic.color = config.Data[worstResult].color;
				}),
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded -= e,
				item =>
				{
					if (item is not IT3JudgeItem judgeItem ||
					    config.Data[judgeItem.JudgeResult].worsePriority <= config.Data[worstResult].worsePriority)
						return;
					worstResult = judgeItem.JudgeResult;
					targetGraphic.color = config.Data[worstResult].color;
				})
		};

		// Private
		private JudgeStorage judgeStorage = default!;

		private T3JudgeResult worstResult;

		// Constructor
		[Inject]
		private void Construct(JudgeStorage judgeStorage)
		{
			this.judgeStorage = judgeStorage;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}