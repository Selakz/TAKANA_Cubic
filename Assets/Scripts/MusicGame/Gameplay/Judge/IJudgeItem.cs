#nullable enable

using System;
using T3Framework.Runtime;

namespace MusicGame.Gameplay.Judge
{
	public interface IJudgeItem
	{
		IComboItem ComboItem { get; }

		T3Time ActualTime { get; set; }
	}

	public interface IHasJudgedInput<T> : IJudgeItem where T : struct
	{
		T? JudgedInput { get; set; }
	}

	public interface IHasJudgeResult<T> : IJudgeItem where T : Enum
	{
		T JudgeResult { get; set; }
	}
}