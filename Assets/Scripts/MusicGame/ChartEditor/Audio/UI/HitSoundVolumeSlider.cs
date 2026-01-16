#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class HitSoundVolumeSlider : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Slider hitSoundVolumeSlider = default!;
		[SerializeField] private TMP_Text hitSoundVolumeText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<int>(ISingleton<PlayfieldSetting>.Instance.HitSoundVolumePercent, () =>
			{
				var percent = ISingleton<PlayfieldSetting>.Instance.HitSoundVolumePercent.Value;
				HitSoundVolumePercent = percent;
			}),
			new SliderRegistrar(hitSoundVolumeSlider,
				value =>
				{
					ISingleton<PlayfieldSetting>.Instance.HitSoundVolumePercent.Value = Mathf.RoundToInt(value);
					hitSoundVolumeText.text = Mathf.RoundToInt(value).ToString();
				})
		};

		// Private
		private int HitSoundVolumePercent
		{
			set
			{
				hitSoundVolumeSlider.SetValueWithoutNotify(value);
				hitSoundVolumeText.text = value.ToString();
			}
		}

		// Defined Functions
		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}