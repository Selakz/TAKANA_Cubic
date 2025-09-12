using T3Framework.Runtime.Event;
using UnityEngine;

namespace MusicGame.Gameplay.Audio
{
	public class HitSoundManager : MonoBehaviour
	{
		// Serializable and Public
		[SerializeField] private AudioSource hitSound;

		public static HitSoundManager Instance { get; private set; }

		public float Volume { get; set; } = 1f;

		// Private
		private double lastHitTime;

		// Static

		// Defined Functions
		private void AudioOnPlayHitSound(float volume)
		{
			if (volume <= 0) return;
			double currentTime = AudioSettings.dspTime;
			if (currentTime - lastHitTime < 0.003) volume *= 0.5f;
			hitSound.PlayOneShot(hitSound.clip, volume * Volume);
			lastHitTime = currentTime;
		}

		// System Functions
		void Awake()
		{
			hitSound.clip = Resources.Load<AudioClip>("Audios/HitSound");
		}

		void OnEnable()
		{
			Instance = this;
			EventManager.Instance.AddListener<float>("Audio_OnPlayHitSound", AudioOnPlayHitSound);
		}

		void OnDisable()
		{
			EventManager.Instance.RemoveListener<float>("Audio_OnPlayHitSound", AudioOnPlayHitSound);
		}
	}
}