using UnityEngine;

// ReSharper disable InconsistentNaming
namespace T3Framework.Runtime.Audio
{
	[RequireComponent(typeof(AudioSource))]
	public class CanNegativeAudioSource : MonoBehaviour
	{
		// Public
		public float time
		{
			get
			{
				if (isPseudoPlaying)
				{
					return (float)(AudioSettings.dspTime - scheduledDspTime);
				}

				return remainingNegativeTime > 0 ? (float)-remainingNegativeTime : audioSource.time;
			}
			set
			{
				if (value < 0)
				{
					audioSource.Stop();
					audioSource.time = 0;
					if (isPlaying)
					{
						isPseudoPlaying = true;
						remainingNegativeTime = 0;
						scheduledDspTime = AudioSettings.dspTime - value;
						audioSource.PlayScheduled(scheduledDspTime);
					}
					else
					{
						isPseudoPlaying = false;
						remainingNegativeTime = -value;
					}
				}
				else
				{
					isPseudoPlaying = false;
					remainingNegativeTime = 0;
					// Time assignment fails when audio ends, stops and jumps to 0, So you need to load it again.
					if (!audioSource.isPlaying && audioSource.time == 0f)
					{
						audioSource.Play();
						audioSource.Pause();
					}

					audioSource.time = value;
				}
			}
		}

		public float pitch
		{
			get => audioSource.pitch;
			set => audioSource.pitch = value;
		}

		public float volume
		{
			get => audioSource.volume;
			set => audioSource.volume = Mathf.Max(0f, Mathf.Min(1f, value));
		}

		public bool isPlaying => isPseudoPlaying || audioSource.isPlaying;

		public AudioClip clip
		{
			get => audioSource.clip;
			set => audioSource.clip = value;
		}

		// Private
		private AudioSource audioSource;
		private double scheduledDspTime; // the time when the audio actually played
		private double remainingNegativeTime; // only be set to positive when time is set to negative and is not playing
		private bool isPseudoPlaying;

		// Defined Functions
		public void Play()
		{
			if (isPlaying) return;
			if (remainingNegativeTime > 0)
			{
				scheduledDspTime = AudioSettings.dspTime + remainingNegativeTime;
				remainingNegativeTime = 0;
				audioSource.Stop();
				audioSource.PlayScheduled(scheduledDspTime);
				isPseudoPlaying = true;
			}
			else
			{
				audioSource.Play();
				isPseudoPlaying = false;
			}
		}

		public void Stop()
		{
			audioSource.Pause(); // IDK why the following time assignments fails when using Stop()
			isPseudoPlaying = false;
			remainingNegativeTime = 0;
		}

		// System Functions
		private void Awake()
		{
			audioSource = GetComponent<AudioSource>();
			remainingNegativeTime = 0;
			isPseudoPlaying = false;
		}

		private void Update()
		{
			if (isPseudoPlaying)
			{
				double currentTime = AudioSettings.dspTime;
				if (currentTime >= scheduledDspTime)
				{
					isPseudoPlaying = false;
				}
			}
		}
	}
}