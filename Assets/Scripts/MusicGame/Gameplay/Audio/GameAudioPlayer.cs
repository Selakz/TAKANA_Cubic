#nullable enable

using System;
using T3Framework.Runtime;
using T3Framework.Runtime.Audio;
using UnityEngine;

namespace MusicGame.Gameplay.Audio
{
	[RequireComponent(typeof(CanNegativeAudioSource))]
	public class GameAudioPlayer : MonoBehaviour, IGameAudioPlayer
	{
		// Serializable and Public
		[SerializeField] private CanNegativeAudioSource audioSource = default!;
		[SerializeField] private GameAudioSetting setting = default!;
		[SerializeField] private bool stopAtEnd = false;

		public event Action? OnPlay;
		public event Action? OnTimeJump;

		public T3Time AudioDeviation { get; set; }

		public T3Time AudioTime
		{
			get
			{
				if (stopAtEnd && audioSource is { time: <= float.Epsilon, isPlaying: false })
				{
					audioSource.Stop();
					audioSource.time = AudioLength;
					return AudioLength;
				}

				return audioSource.time;
			}
			set
			{
				audioSource.time = Mathf.Clamp(value, setting.timeBeforePlaying, AudioLength) / 1000f;
				OnTimeJump?.Invoke();
			}
		}

		public T3Time ChartTime
		{
			get => AudioTime + AudioDeviation - Offset;
			set => AudioTime = value - AudioDeviation + Offset;
		}

		public T3Time AudioLength => audioSource.clip is { length: var len } ? len : 0f;

		public T3Time Offset { get; set; }

		public float Volume
		{
			get => audioSource.volume;
			set => audioSource.volume = Mathf.Max(0f, Mathf.Min(1f, value));
		}

		public float Pitch
		{
			get => audioSource.pitch;
			set => audioSource.pitch = value;
		}

		public bool IsPlaying => audioSource.isPlaying;

		public AudioClip Clip => audioSource.clip;

		// Private
		private bool isCheckingDelayed = false;

		// Static

		// Defined Functions
		public void Load(AudioClip clip, T3Time offset)
		{
			audioSource.clip = clip;
			audioSource.time = (T3Time)setting.timeBeforePlaying;
			Offset = offset;
		}

		/// <summary>
		/// When <see cref="OnPlay"/> is invoked, the music may not actually start playing.
		/// To avoid this, use <see cref="PlayDelayed"/> instead.
		/// </summary>
		public void Play()
		{
			if (IsPlaying) return;
			audioSource.Play();
			OnPlay?.Invoke();
		}

		/// <summary>
		/// Do not use current <see cref="Time.realtimeSinceStartup"/> + delay to expect the audio's starting time,
		/// but subscribe to <see cref="OnPlay"/> to get the realtime at that time to align with the audio time at that time.
		/// </summary>
		public void PlayDelayed(T3Time delay)
		{
			if (IsPlaying) return;
			audioSource.PlayDelayed(delay.Second);
			isCheckingDelayed = true;
		}

		public void Pause()
		{
			if (!IsPlaying) return;
			var time = audioSource.time;
			audioSource.Stop();
			audioSource.time = time;
			isCheckingDelayed = false;
		}

		public T3Time AudioToChart(T3Time audioTime) => audioTime + setting.audioDeviation - Offset;

		// System Functions
		void Awake()
		{
			AudioDeviation = setting.audioDeviation;
		}

		void Update()
		{
			if (isCheckingDelayed && audioSource.isPlaying)
			{
				OnPlay?.Invoke();
				isCheckingDelayed = false;
			}
		}
	}
}