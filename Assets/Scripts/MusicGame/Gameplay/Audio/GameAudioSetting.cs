using UnityEngine;

namespace MusicGame.Gameplay.Audio
{
	[CreateAssetMenu(fileName = "Audio Setting", menuName = "ScriptableObjects/Audio Setting")]
	public class GameAudioSetting : ScriptableObject
	{
		public int audioDeviation;

		public int timeBeforePlaying;
	}
}