#nullable enable

using UnityEngine.InputSystem.EnhancedTouch;

namespace MusicGame.Gameplay.Judge.T3
{
	public interface IT3JudgeItem : IJudgeItem, IHasJudgedInput<Touch>, IHasJudgeResult<T3JudgeResult>
	{
	}
}