#nullable enable

using T3Framework.Runtime;
using UnityEngine.InputSystem.EnhancedTouch;

namespace MusicGame.Gameplay.Judge.T3
{
	public interface IT3JudgeItem : IJudgeItem
	{
		Touch? JudgedTouch { get; set; }

		T3JudgeResult JudgeResult { get; set; }
	}
}