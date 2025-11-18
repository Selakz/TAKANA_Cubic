#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.Level.UI
{
	public class TimingSlider : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private NotifiableDataContainer<LevelInfo?> levelInfoContainer = default!;
		[SerializeField] private PointerUpDownListener pointerUpDownListener = default!;
		[SerializeField] private Slider slider = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<LevelInfo?>(levelInfoContainer, (_, _) =>
			{
				var levelInfo = levelInfoContainer.Property.Value;
				if (levelInfo is null) slider.interactable = false;
				else
				{
					slider.interactable = true;
					slider.minValue = 0;
					slider.maxValue = Mathf.Floor(levelInfo.Music!.length * 1000) - 1;
					slider.wholeNumbers = true;
				}
			}),
			new SliderRegistrar(slider, _ => { EventManager.Instance.Invoke("Level_OnReset", SliderTime); }),
			new CustomRegistrar(
				() => pointerUpDownListener.PointerDown += OnPointerDown,
				() => pointerUpDownListener.PointerDown -= OnPointerDown),
			new CustomRegistrar(
				() => pointerUpDownListener.PointerUp += OnPointerUp,
				() => pointerUpDownListener.PointerUp -= OnPointerUp)
		};

		// Private
		private T3Time SliderTime => (int)slider.value;

		private bool isPointerDown;

		// Event Handlers
		private void OnPointerDown(PointerEventData _)
		{
			isPointerDown = true;
		}

		private void OnPointerUp(PointerEventData _)
		{
			isPointerDown = false;
		}

		// System Function
		void Update()
		{
			if (slider.interactable && !isPointerDown)
			{
				slider.SetValueWithoutNotify(LevelManager.Instance.Music.AudioTime.Milli);
			}
		}
	}
}