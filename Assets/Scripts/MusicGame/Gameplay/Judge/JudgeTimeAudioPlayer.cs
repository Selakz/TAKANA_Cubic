#nullable enable

using System;
using MusicGame.Gameplay.Audio;
using T3Framework.Runtime;
using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Gameplay.Judge
{
	public class JudgeTimeAudioPlayer : T3MonoBehaviour, IGameAudioPlayer
	{
		// Serializable and Public
		[SerializeField] private GameAudioPlayer gameAudioPlayer = default!;

		public event Action? OnPlay;

		public event Action? OnTimeJump;

		public T3Time AudioDeviation
		{
			get => gameAudioPlayer.AudioDeviation;
			set => gameAudioPlayer.AudioDeviation = value;
		}

		public T3Time AudioTime
		{
			get => ChartTime - AudioDeviation + Offset;
			set => gameAudioPlayer.AudioTime = value;
		}

		public T3Time ChartTime
		{
			get => chartTime is null || gameAudioPlayer.AudioTime == AudioLength
				? gameAudioPlayer.ChartTime
				: chartTime.Value.Second + (float)(Time.realtimeSinceStartupAsDouble - pacedStartUnityTime);
			set => AudioTime = value - AudioDeviation + Offset;
		}

		public T3Time AudioLength => gameAudioPlayer.AudioLength;

		public T3Time Offset
		{
			get => gameAudioPlayer.Offset;
			set => gameAudioPlayer.Offset = value;
		}

		public float Volume
		{
			get => gameAudioPlayer.Volume;
			set => gameAudioPlayer.Volume = value;
		}

		public float Pitch
		{
			get => gameAudioPlayer.Pitch;
			set => gameAudioPlayer.Pitch = value;
		}

		public AudioClip Clip => gameAudioPlayer.Clip;
		public bool IsPlaying => gameAudioPlayer.IsPlaying;

		// Event Registrars
		protected override IEventRegistrar[] EnableRegistrars => new IEventRegistrar[]
		{
			CustomRegistrar.Generic<Action>(e => gameAudioPlayer.OnPlay += e,
				e => gameAudioPlayer.OnPlay -= e,
				OnAudioPlay),
			CustomRegistrar.Generic<Action>(e => gameAudioPlayer.OnTimeJump += e,
				e => gameAudioPlayer.OnTimeJump -= e,
				OnAudioTimeJump),
		};

		public void Load(AudioClip clip, T3Time offset) => gameAudioPlayer.Load(clip, offset);

		public void Pause() => gameAudioPlayer.Pause();

		public void Play() => gameAudioPlayer.Play();

		public T3Time AudioToChart(T3Time audioTime) => gameAudioPlayer.AudioToChart(audioTime);

		// Private
		private float updatePace = 1;
		private T3Time? chartTime;
		private double startDspTime;
		private double startUnityTime;
		private double pacedStartUnityTime;

		// Defined Functions
		public void Align(T3Time chartTime, double dspTime, double unityTime)
		{
			this.chartTime = chartTime;
			startDspTime = dspTime;
			startUnityTime = unityTime;
			pacedStartUnityTime = unityTime;
			updatePace = 1;
		}

		public T3Time GetChartTime(double inputTime)
		{
			var inputChartTime = chartTime is null || gameAudioPlayer.AudioTime == AudioLength
				? gameAudioPlayer.ChartTime.Second
				: chartTime.Value.Second + (float)(inputTime - pacedStartUnityTime);
			return inputChartTime;
		}

		// Event Handlers
		private void OnAudioPlay()
		{
			OnPlay?.Invoke();
			Align(gameAudioPlayer.ChartTime, AudioSettings.dspTime, Time.realtimeSinceStartupAsDouble);
		}

		private void OnAudioTimeJump()
		{
			OnTimeJump?.Invoke();
			Align(gameAudioPlayer.ChartTime, AudioSettings.dspTime, Time.realtimeSinceStartupAsDouble);
		}

		// System Functions
		void Update()
		{
			// Trick from https://github.com/Arcthesia/ArcCreate
			var dspElapsed = AudioSettings.dspTime - startDspTime;
			var unityElapsed = Time.realtimeSinceStartupAsDouble - startUnityTime;
			updatePace = unityElapsed < 0 + Mathf.Epsilon
				? 1
				: Mathf.Lerp(updatePace, (float)(dspElapsed / unityElapsed), 0.1f);
			unityElapsed *= updatePace;
			pacedStartUnityTime = Time.realtimeSinceStartupAsDouble - unityElapsed;
		}
	}
}