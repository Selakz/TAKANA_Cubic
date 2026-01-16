#nullable enable

using System;
using System.Collections.Generic;
using T3Framework.Runtime;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Judge.T3
{
	[Serializable]
	public struct Interval
	{
		public int startTime;
		public int endTime;
	}

	[CreateAssetMenu(fileName = "T3JudgeConfig", menuName = "T3GameplayConfig/T3JudgeConfig")]
	public class T3JudgeConfig : ScriptableObject
	{
		[SerializeField] private InspectorDictionary<T3JudgeResult, Interval> resultMap = default!;

		public IReadOnlyDictionary<T3JudgeResult, Interval> ResultMap => resultMap.Value;

		public bool IsInJudgeRange(T3Time timeJudge, T3Time timeInput, out T3JudgeResult judgeResult)
		{
			var distance = timeInput - timeJudge;
			foreach (var (result, interval) in ResultMap)
			{
				if (interval.startTime <= distance && interval.endTime >= distance)
				{
					judgeResult = result;
					return true;
				}
			}

			judgeResult = default;
			return false;
		}
	}
}