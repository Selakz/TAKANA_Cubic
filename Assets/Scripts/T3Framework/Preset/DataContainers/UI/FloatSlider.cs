#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;
using UnityEngine.UI;

namespace T3Framework.Preset.DataContainers.UI
{
	public class FloatSlider : T3MonoBehaviour
	{
		[SerializeField] private NotifiableDataContainer<float> floatDataContainer = default!;
		[SerializeField] private float sliderDisplayScale = 1f;
		[SerializeField] private Slider slider = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new DataContainerRegistrar<float>(floatDataContainer,
				(_, _) => { slider.SetValueWithoutNotify(floatDataContainer.Property.Value * sliderDisplayScale); }),
			new SliderRegistrar(slider, value => floatDataContainer.Property.Value = value / sliderDisplayScale)
		};
	}
}