#nullable enable

using System;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Movement;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.UI
{
	[Serializable]
	public struct FastLateData
	{
		public string description;
		public Color color;
	}

	public class FastLateIndicator : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private T3JudgeResultConfig config = default!;
		[SerializeField] private FastLateData fastData;
		[SerializeField] private FastLateData lateData;
		[SerializeField] private TextMeshProUGUI text = default!;
		[SerializeField] private FloatMovementContainer movement = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(
				e => judgeStorage.OnJudgeItemCleared += e,
				e => judgeStorage.OnJudgeItemCleared -= e,
				() => text.gameObject.SetActive(false)),
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded -= e,
				item =>
				{
					if (item is not IT3JudgeItem judgeItem) return;
					switch (config.Data[judgeItem.JudgeResult].fastLateStatus)
					{
						case 0:
							return;
						case < 0:
							text.text = fastData.description;
							text.color = fastData.color;
							break;
						case > 0:
							text.text = lateData.description;
							text.color = lateData.color;
							break;
					}

					text.gameObject.SetActive(true);
					movement.Move(
						() => text.transform.localScale.x,
						value => text.transform.localScale = new(value, value, 1));
					hideAction.Invoke(() => text.gameObject.SetActive(false), movement.Length);
				})
		};

		// Private
		private JudgeStorage judgeStorage = default!;

		private readonly DebounceAction hideAction = new();

		// Constructor
		[Inject]
		private void Construct(JudgeStorage judgeStorage)
		{
			this.judgeStorage = judgeStorage;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}