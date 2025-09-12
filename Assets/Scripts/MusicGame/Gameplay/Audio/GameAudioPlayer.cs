using T3Framework.Runtime;
using T3Framework.Runtime.Audio;
using UnityEngine;

namespace MusicGame.Gameplay.Audio
{
	[RequireComponent(typeof(CanNegativeAudioSource))]
	public class GameAudioPlayer : MonoBehaviour, IGameAudioPlayer
	{
		// Serializable and Public
		[SerializeField] private CanNegativeAudioSource audioSource;
		[SerializeField] private GameAudioSetting setting;

		public T3Time AudioTime
		{
			get => audioSource.time;
			set => audioSource.time = (T3Time)Mathf.Max(setting.timeBeforePlaying, Mathf.Min(AudioLength, value));
		}

		public T3Time ChartTime
		{
			get => AudioTime - setting.audioDeviation - Offset;
			set => AudioTime = value + setting.audioDeviation + Offset;
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

		// Static

		// Defined Functions
		public void Load(AudioClip clip, T3Time offset)
		{
			audioSource.clip = clip;
			audioSource.time = (T3Time)setting.timeBeforePlaying;
			Offset = offset;
		}

		public void Play()
		{
			if (IsPlaying) return;
			audioSource.Play();
		}

		public void Pause()
		{
			if (!IsPlaying) return;
			var time = audioSource.time;
			audioSource.Stop();
			audioSource.time = time;
		}
	}
}