using System;
using T3Framework.Runtime;
using UnityEngine;

namespace MusicGame.Gameplay.Audio
{
	public interface IGameAudioPlayer
	{
		public event Action OnPlay;

		public event Action OnTimeJump;

		public T3Time AudioDeviation { get; set; }

		public T3Time ChartTime { get; set; }

		public T3Time AudioTime { get; set; }

		public T3Time AudioLength { get; }

		public T3Time Offset { get; set; }

		public float Volume { get; set; }

		public float Pitch { get; set; }

		public AudioClip Clip { get; }

		public bool IsPlaying { get; }

		public void Load(AudioClip clip, T3Time offset);

		public void Pause();

		public void Play();

		public T3Time AudioToChart(T3Time audioTime);
	}
}