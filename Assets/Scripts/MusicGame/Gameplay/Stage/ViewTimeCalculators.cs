#nullable enable

using System;
using System.Collections.Generic;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Level;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.Gameplay.Stage
{
	public class T3ViewTimeCalculator : ITimeCalculator<ChartComponent>
	{
		public T3Time GetTimeInstantiate(ChartComponent? item)
		{
			if (item is null) return T3Time.MinValue;
			return item.Model switch
			{
				INote note =>
					// Note: At least one of them is T3Time.MinValue
					Mathf.Max(note.Movement.FirstTimeWhen(ISingleton<PlayfieldSetting>.Instance.UpperThreshold, true),
						note.Movement.FirstTimeWhen(ISingleton<PlayfieldSetting>.Instance.LowerThreshold, false)),
				ITrack track => track.TimeStart,
				StaticJudgeLine line => line.TimeMin,
				_ => T3Time.MinValue
			};
		}

		public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime)
		{
			return Mathf.Max(selfTime, parentTime);
		}

		public T3Time GetTimeDestroy(ChartComponent? item)
		{
			if (item is null) return T3Time.MaxValue;
			return item.Model switch
			{
				Hit hit => hit.TimeJudge + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				Hold hold => hold.TimeEnd + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				ITrack track => track.TimeEnd,
				StaticJudgeLine line => line.TimeMax,
				_ => T3Time.MaxValue
			};
		}

		public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime)
		{
			return Mathf.Min(selfTime, parentTime);
		}

		public ChartComponent? GetParent(ChartComponent item) => item.Parent;

		public IEnumerable<ChartComponent> GetChildren(ChartComponent item) => item.Children;
	}

	public class FallingViewTimeCalculator : ITimeCalculator<ChartComponent>
	{
		public T3Time GetTimeInstantiate(ChartComponent? item)
		{
			if (item is null) return T3Time.MinValue;
			return item.Model switch
			{
				INote note =>
					// Note: At least one of them is T3Time.MinValue
					Mathf.Max(note.Movement.FirstTimeWhen(ISingleton<PlayfieldSetting>.Instance.UpperThreshold, true),
						note.Movement.FirstTimeWhen(ISingleton<PlayfieldSetting>.Instance.LowerThreshold, false)),
				ITrack track => track.TimeStart - ISingleton<PlayfieldSetting>.Instance.UpperThreshold,
				StaticJudgeLine line => line.TimeMin,
				_ => T3Time.MinValue
			};
		}

		public T3Time InstantiateTimeRestriction(T3Time selfTime, T3Time parentTime)
		{
			return Mathf.Max(selfTime, parentTime);
		}

		public T3Time GetTimeDestroy(ChartComponent? item)
		{
			if (item is null) return T3Time.MaxValue;
			return item.Model switch
			{
				Hit hit => hit.TimeJudge + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				Hold hold => hold.TimeEnd + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				ITrack track => track.TimeEnd + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				StaticJudgeLine line => line.TimeMax,
				_ => T3Time.MaxValue
			};
		}

		public T3Time DestroyTimeRestriction(T3Time selfTime, T3Time parentTime)
		{
			return Mathf.Min(selfTime, parentTime);
		}

		public ChartComponent? GetParent(ChartComponent item) => item.Parent;

		public IEnumerable<ChartComponent> GetChildren(ChartComponent item) => item.Children;
	}

	public static class ViewTimeCalculatorExtension
	{
		private static readonly T3ViewTimeCalculator t3ViewTimeCalculator = new();
		private static readonly FallingViewTimeCalculator fallingViewTimeCalculator = new();

		public static ITimeCalculator<ChartComponent> GetViewTimeCalculator(this GameplayStageSkinConfig skinConfig)
		{
			return skinConfig.trackBehaviour switch
			{
				TrackBehaviour.Instant => t3ViewTimeCalculator,
				TrackBehaviour.Falling => fallingViewTimeCalculator,
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}