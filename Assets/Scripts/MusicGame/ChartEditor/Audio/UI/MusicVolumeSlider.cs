#nullable enable

using MusicGame.ChartEditor.Level;
using MusicGame.Gameplay.Audio;
using MusicGame.Gameplay.Level;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using T3Framework.Static.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class MusicVolumeSlider : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Slider musicVolumeSlider = default!;
		[SerializeField] private TMP_Text musicVolumeText = default!;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new PropertyRegistrar<LevelInfo?>(levelInfo, (_, _) =>
			{
				var info = levelInfo.Value;
				if (info?.Preference is EditorPreference preference)
				{
					MusicVolumePercent = preference.MusicVolumePercent;
				}
			}),
			new SliderRegistrar(musicVolumeSlider, value =>
			{
				music.Volume = value / 100f;
				musicVolumeText.text = Mathf.RoundToInt(value).ToString();

				var info = levelInfo.Value;
				if (info?.Preference is EditorPreference preference)
				{
					preference.MusicVolumePercent = Mathf.RoundToInt(value);
				}
			})
		};

		// Private
		private GameAudioPlayer music = default!;
		private NotifiableProperty<LevelInfo?> levelInfo = default!;

		private int MusicVolumePercent
		{
			set
			{
				musicVolumeSlider.SetValueWithoutNotify(value);
				musicVolumeText.text = value.ToString();
			}
		}

		// Defined Functions
		[Inject]
		private void Construct(
			NotifiableProperty<LevelInfo?> levelInfo,
			GameAudioPlayer music)
		{
			this.levelInfo = levelInfo;
			this.music = music;
		}

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);
	}
}