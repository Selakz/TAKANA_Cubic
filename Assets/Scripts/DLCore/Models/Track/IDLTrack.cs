#nullable enable

using MusicGame.Models.Track;
using T3Framework.Static.Movement;

namespace DLCore.Models.Track
{
	public interface IDLTrack : ITrack
	{
		public IMovement<ColorVariantGradient> ColorMovement { get; set; }
	}
}