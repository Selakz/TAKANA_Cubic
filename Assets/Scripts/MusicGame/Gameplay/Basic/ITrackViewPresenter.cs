#nullable enable

using T3Framework.Runtime;

namespace MusicGame.Gameplay.Basic
{
	public interface ITrackViewPresenter
	{
		public Modifier<float> PositionModifier { get; }

		public Modifier<float> WidthModifier { get; }
	}
}