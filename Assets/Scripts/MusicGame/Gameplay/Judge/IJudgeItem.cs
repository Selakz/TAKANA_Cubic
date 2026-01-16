#nullable enable

using T3Framework.Runtime;

namespace MusicGame.Gameplay.Judge
{
	public interface IJudgeItem
	{
		IComboItem ComboItem { get; }

		T3Time ActualTime { get; set; }
	}
}