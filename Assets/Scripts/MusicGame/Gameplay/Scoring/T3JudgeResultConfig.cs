#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime.Serialization.Inspector;
using UnityEngine;

namespace MusicGame.Gameplay.Scoring
{
	[Serializable]
	public struct FastLateData
	{
		public string description;
		public Color color;
		public int offsetMilli;
	}

	[Serializable]
	public struct T3JudgeResultData
	{
		public double scoreRate;

		public Color color;

		/// <summary> negative: fast, 0: not show, positive: late </summary>
		public int fastLateStatus;

		/// <summary> indicates how "bad" is this judge result </summary>
		public int worsePriority;
	}

	[CreateAssetMenu(fileName = "T3JudgeResultConfig", menuName = "T3GameplayConfig/JudgeResultConfig")]
	public class T3JudgeResultConfig : ScriptableObject
	{
		[SerializeField] private InspectorDictionary<T3JudgeResult, T3JudgeResultData> resultData = default!;
		[SerializeField] private List<T3JudgeResult> offComboResults = default!;
		[SerializeField] private FastLateData fastSampleData;
		[SerializeField] private FastLateData lateSampleData;

		public Dictionary<T3JudgeResult, T3JudgeResultData> Data => resultData.Value;

		public bool IsOffCombo(T3JudgeResult result) => offComboResults.Contains(result);

		public FastLateData GetFastLateData(IT3JudgeItem judgeItem)
		{
			return judgeItem.ActualTime < judgeItem.ComboItem.ExpectedTime
				? fastSampleData with { offsetMilli = judgeItem.ActualTime - judgeItem.ComboItem.ExpectedTime }
				: lateSampleData with { offsetMilli = judgeItem.ActualTime - judgeItem.ComboItem.ExpectedTime };
		}
	}
}