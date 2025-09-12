using MusicGame.Components.Tracks.Movement;

namespace MusicGame.Components.Tracks
{
	/// <summary>
	/// 标识某个元件属于Track类
	/// </summary>
	public interface ITrack : IComponent
	{
		public float Width { get; }

		public ITrackMovement Movement { get; set; }
	}
}