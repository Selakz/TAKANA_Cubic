#nullable enable

using T3Framework.Runtime.Event;

namespace MusicGame.Gameplay.Level
{
	public class LevelInfoDataContainer : NotifiableDataContainer<LevelInfo?>
	{
		public override LevelInfo? InitialValue => null;
	}
}