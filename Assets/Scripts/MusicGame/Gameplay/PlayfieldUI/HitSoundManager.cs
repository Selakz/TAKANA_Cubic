using UnityEngine;

public class HitSoundManager : MonoBehaviour
{
	// Serializable and Public
	[SerializeField] private AudioSource hitSound;

	public static HitSoundManager Instance { get; private set; }

	public float Volume { get; set; } = 1.0f;

	// Private
	private double lastHitTime;

	// Static

	// Defined Functions
	public void PlayHitSound(object volume)
	{
		if (Volume <= 0) return;
#if TAKANA_EDITOR
		if (EditingLevelManager.Instance.IsPaused) return;
#endif
		float v = (float)volume;
		double currentTime = AudioSettings.dspTime;
		if (currentTime - lastHitTime < 0.003) v = 0.5f;
		hitSound.PlayOneShot(hitSound.clip, v * Volume);
		lastHitTime = currentTime;
	}

	// System Functions
	void Awake()
	{
		Instance = this;
		hitSound.clip = MyResources.Load<AudioClip>("Audios/HitSound");
		EventManager.AddListener(EventManager.EventName.PlayHitSound, PlayHitSound);

		int effectVolume = EditingLevelManager.Instance.GlobalSetting.HitSoundVolumePercent;
		Volume = effectVolume / 100f;
	}
}