#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Movement;
using T3Framework.Runtime.Serialization.Inspector;
using T3Framework.Runtime.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.Gameplay.Scoring.UI
{
	public class JudgeIndicator : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private InspectorDictionary<T3JudgeResult, Sprite> judgeTextures = new();
		[SerializeField] private FloatMovementContainer movement = default!;
		[SerializeField] private List<Image> judgeImages = default!;

		// Private
		private JudgeStorage judgeStorage = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action<IJudgeItem>>(
				e => judgeStorage.OnJudgeItemAdded += e,
				e => judgeStorage.OnJudgeItemAdded -= e,
				item =>
				{
					if (item is not IT3JudgeItem judgeItem) return;
					if (!judgeTextures.Value.TryGetValue(judgeItem.JudgeResult, out var texture)) return;
					foreach (var judgeImage in judgeImages)
					{
						judgeImage.enabled = true;
						judgeImage.sprite = texture;
					}

					if (judgeImages.Count > 0)
					{
						movement.Move(
							() => judgeImages[0].transform.localScale.x,
							value =>
							{
								foreach (var judgeImage in judgeImages)
								{
									judgeImage.transform.localScale = new(value, value, 1);
								}
							});
					}
				}),
			CustomRegistrar.Generic<Action>(
				e => judgeStorage.OnJudgeItemCleared += e,
				e => judgeStorage.OnJudgeItemCleared -= e,
				() =>
				{
					foreach (var judgeImage in judgeImages) judgeImage.enabled = false;
				})
		};

		// Constructor
		[Inject]
		private void Construct(JudgeStorage judgeStorage)
		{
			this.judgeStorage = judgeStorage;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}