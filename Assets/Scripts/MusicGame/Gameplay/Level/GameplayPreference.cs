#nullable enable

using T3Framework.Runtime;
using T3Framework.Runtime.Setting;

namespace MusicGame.Gameplay.Level
{
	public class GameplayPreference : IPreference, ISetting<GameplayPreference>
	{
		public int Difficulty { get; set; } = 0;

		public T3Time SongDeviation { get; set; } = 0;
	}
}