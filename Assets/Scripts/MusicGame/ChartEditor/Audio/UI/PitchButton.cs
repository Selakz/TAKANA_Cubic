using System.Collections.Generic;
using MusicGame.Gameplay.Audio;
using T3Framework.Preset.Event;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using T3Framework.Runtime.VContainer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace MusicGame.ChartEditor.Audio.UI
{
	public class PitchButton : T3MonoBehaviour, ISelfInstaller
	{
		// Serializable and Public
		[SerializeField] private Button pitchButton;
		[SerializeField] private TMP_Text speedText;

		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			new ButtonRegistrar(pitchButton, OnPitchButtonClick)
		};

		// Private
		private IGameAudioPlayer music;
		private readonly List<float> speeds = new() { 1.00f, 0.75f, 0.50f, 0.25f }; // TODO: Extract to setting
		private int speedIndex = 0;

		private float Pitch
		{
			set => speedText.text = $"{Mathf.RoundToInt(value * 100)}%";
		}

		// Defined Functions
		[Inject]
		private void Construct(IGameAudioPlayer music) => this.music = music;

		public void SelfInstall(IContainerBuilder builder) => builder.RegisterComponent(this);

		// Event Handlers
		private void OnPitchButtonClick()
		{
			speedIndex = (speedIndex + 1) % speeds.Count;
			music.Pitch = speeds[speedIndex];
			Pitch = music.Pitch;
		}
	}
}