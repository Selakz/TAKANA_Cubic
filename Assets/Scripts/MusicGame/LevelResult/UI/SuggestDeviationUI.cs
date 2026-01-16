#nullable enable

using MusicGame.Gameplay.Judge.T3;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.LevelResult.UI
{
	public class SuggestDeviationUI : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private TextMeshProUGUI levelDeviationText = default!;
		[SerializeField] private TextMeshProUGUI suggestDeviationText = default!;
		[SerializeField] private Button setDeviationButton = default!;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<ResultInfo?>(resultInfo, UpdateDataAndUI),
			new ButtonRegistrar(setDeviationButton,
				() =>
				{
					ISingletonSetting<PlayfieldSetting>.Instance.AudioDeviation.Value = suggestDeviation;
					ISingletonSetting<PlayfieldSetting>.SaveInstance();
				})
		};

		// Private
		private NotifiableProperty<ResultInfo?> resultInfo = default!;

		private T3Time suggestDeviation;

		// Constructor
		[Inject]
		private void Construct(NotifiableProperty<ResultInfo?> resultInfo)
		{
			this.resultInfo = resultInfo;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Defined Functions
		public void UpdateDataAndUI()
		{
			if (resultInfo.Value?.JudgeItems is not { } judgeItems)
			{
				levelDeviationText.text = string.Empty;
				suggestDeviationText.text = string.Empty;
				setDeviationButton.interactable = false;
			}
			else
			{
				T3Time totalDeviation = 0;
				int totalCount = 0;
				foreach (var item in judgeItems)
				{
					if (item is IT3JudgeItem { JudgeResult: T3JudgeResult.EarlyMiss or T3JudgeResult.LateMiss })
					{
						continue;
					}

					totalDeviation += item.ActualTime - item.ComboItem.ExpectedTime;
					totalCount++;
				}

				var levelDeviation = totalCount is 0 ? 0 : totalDeviation.Milli / totalCount;
				suggestDeviation = ISingleton<PlayfieldSetting>.Instance.AudioDeviation.Value + levelDeviation;

				levelDeviationText.text = levelDeviation.ToString();
				suggestDeviationText.text = suggestDeviation.ToString();
				setDeviationButton.interactable = true;
			}
		}
	}
}