#nullable enable

using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.Setting;
using T3Framework.Static;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MusicGame.ChartEditor.TrackLayer.UI
{
	public class TrackOpacitySlider : T3MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private bool isToSelected = true;
		[SerializeField] private Slider trackOpacitySlider = default!;
		[SerializeField] private TMP_Text trackOpacityText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<float>(Opacity, () =>
			{
				var value = Opacity.Value * 100;
				trackOpacitySlider.SetValueWithoutNotify(value);
				trackOpacityText.text = Mathf.RoundToInt(value).ToString();
			}),
			new SliderRegistrar(trackOpacitySlider, value =>
			{
				Opacity.Value = value / 100f;
				ISingletonSetting<TrackLayerSetting>.SaveInstance();
				trackOpacityText.text = Mathf.RoundToInt(value).ToString();
			})
		};

		// Private
		private NotifiableProperty<float> Opacity => isToSelected
			? ISingleton<TrackLayerSetting>.Instance.SelectLayerOpacityRatio
			: ISingleton<TrackLayerSetting>.Instance.UnselectLayerOpacityRatio;
	}
}