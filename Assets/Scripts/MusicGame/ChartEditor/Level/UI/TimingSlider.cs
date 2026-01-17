#nullable enable

using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Event.UI;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Level.UI
{
	public class TimingSlider : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private PointerUpDownListener pointerUpDownListener = default!;
		[SerializeField] private Slider slider = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				if (info is null) slider.interactable = false;
				else
				{
					slider.interactable = true;
					slider.minValue = 0;
					slider.maxValue = Mathf.Floor(info.Music!.length * 1000) - 1;
					slider.wholeNumbers = true;
				}
			}),
			new SliderRegistrar(slider, _ => { music.AudioTime = SliderTime; }),
			new CustomRegistrar(
				() => pointerUpDownListener.PointerDown += OnPointerDown,
				() => pointerUpDownListener.PointerDown -= OnPointerDown),
			new CustomRegistrar(
				() => pointerUpDownListener.PointerUp += OnPointerUp,
				() => pointerUpDownListener.PointerUp -= OnPointerUp)
		};

		// Private
		private NotifiableProperty<LevelInfo?> levelInfo = default!;
		private IGameAudioPlayer music = default!;
		private T3Time SliderTime => (int)slider.value;
		private bool isPointerDown;

		// Defined Functions
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			IGameAudioPlayer music)
		{
			this.levelInfo = levelInfo;
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnPointerDown(PointerEventData _) => isPointerDown = true;

		private void OnPointerUp(PointerEventData _) => isPointerDown = false;

		// System Function
		void Update()
		{
			if (slider.interactable && !isPointerDown)
			{
				slider.SetValueWithoutNotify(music.AudioTime.Milli);
			}
		}
	}
}