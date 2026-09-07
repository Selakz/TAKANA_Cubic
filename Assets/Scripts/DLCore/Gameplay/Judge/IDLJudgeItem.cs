#nullable enable

using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;

namespace DLCore.Gameplay.Judge
{
	public interface IDLJudgeItem : IJudgeItem, IHasJudgeResult<T3JudgeResult>
	{
	}
}
