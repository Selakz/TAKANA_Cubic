using T3Framework.Runtime;

namespace MusicGame.Gameplay.Audio
{
	public interface IGameAudioPlayer
	{
		public T3Time ChartTime { get; set; }

		public T3Time AudioTime { get; set; }

		public T3Time AudioLength { get; }

		public T3Time Offset { get; set; }

		public float Volume { get; set; }

		public float Pitch { get; set; }
	}
}