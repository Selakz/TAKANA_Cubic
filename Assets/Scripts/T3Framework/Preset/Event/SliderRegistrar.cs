using T3Framework.Runtime.Event;
using UnityEngine.Events;
using UnityEngine.UI;

namespace T3Framework.Preset.Event
{
	public readonly struct SliderRegistrar : IEventRegistrar
	{
		private readonly Slider slider;
		private readonly UnityAction<float> action;

		public SliderRegistrar(Slider slider, UnityAction<float> action)
		{
			this.slider = slider;
			this.action = action;
		}

		public void Register()
		{
			slider.onValueChanged.AddListener(action);
			action.Invoke(slider.value);
		}

		public void Unregister()
		{
			slider.onValueChanged.RemoveListener(action);
		}
	}
}