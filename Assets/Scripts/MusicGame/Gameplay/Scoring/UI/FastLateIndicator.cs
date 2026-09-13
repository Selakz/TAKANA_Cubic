#nullable enable

using System;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Movement;
using T3Framework.Runtime.Threading;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.UI
{
	public class FastLateIndicator : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private T3JudgeResultConfig config = default!;
		[SerializeField] private TextMeshProUGUI text = default!;
		[SerializeField] private TextMeshProUGUI offsetText = default!;
		[SerializeField] private FloatMovementContainer movement = default!;
		[SerializeField] private int lightThresholdAbsMilli = 20;
		[SerializeField] private float lightAlpha = 0.25f;

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
					bool isDetailed = ISingleton<PlayfieldSetting>.Instance.DetailedFastLateIndicator &&
					                  !config.IsOffCombo(judgeItem.JudgeResult);
					if (!isDetailed && config.Data[judgeItem.JudgeResult].fastLateStatus == 0) return;
					var fastLateData = config.GetFastLateData(judgeItem);
					if (fastLateData.offsetMilli == 0) return;

					text.text = fastLateData.description;
					var color = Mathf.Abs(fastLateData.offsetMilli) < lightThresholdAbsMilli
						? fastLateData.color with { a = lightAlpha }
						: fastLateData.color;
					text.color = color;
					if (isDetailed)
					{
						offsetText.text = $"{fastLateData.offsetMilli:+0;-0;0}ms";
						offsetText.color = color;
					}

					text.gameObject.SetActive(true);
					if (isDetailed) offsetText.gameObject.SetActive(true);

					movement.Move(
						() => text.transform.localScale.x,
						value =>
						{
							text.transform.localScale = new(value, value, 1);
							if (isDetailed) offsetText.transform.localScale = new(value, value, 1);
						});
					hideAction.Invoke(() =>
					{
						text.gameObject.SetActive(false);
						if (isDetailed) offsetText.gameObject.SetActive(false);
					}, movement.Length);
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