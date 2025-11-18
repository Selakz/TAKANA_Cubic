#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.Gameplay.Speed;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class SpeedSlider : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<LevelInfo?> levelInfoContainer = default!;
		[SerializeField] private SpeedDataContainer speedContainer = default!;
		[SerializeField] private Slider speedSlider = default!;
		[SerializeField] private TMP_Text speedText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<LevelInfo?>(levelInfoContainer, (_, _) =>
			{
				var levelInfo = levelInfoContainer.Property.Value;
				speedSlider.interactable = levelInfo is not null;
			}),
			new DataContainerRegistrar<ISpeed>(speedContainer, (_, _) =>
			{
				speedSlider.SetValueWithoutNotify(speedContainer.Speed * 10);
				speedText.text = speedContainer.Speed.ToString("0.0");
			}),
			new SliderRegistrar(speedSlider, value =>
			{
				float speed = value / 10;
				speedContainer.Speed = speed;
			})
		};
	}
}