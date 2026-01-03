#nullable enable

using MusicGame.Gameplay.Level;
using MusicGame.Models;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Track;
using T3Framework.Runtime;
using T3Framework.Static;
using UnityEngine;

namespace MusicGame.Gameplay.Stage
{
	/// <summary>
	/// Temporary gathers all logic here
	/// </summary>
	public class T3GeneralViewTimeCalculator : IModelTimeCalculator, ISingleton<T3GeneralViewTimeCalculator>
	{
		public T3Time GetTimeInstantiate(IChartModel? model)
		{
			return model switch
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

		public T3Time GetTimeDestroy(IChartModel? model)
		{
			return model switch
			{
				Hit hit => hit.TimeJudge + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				Hold hold => hold.TimeEnd + ISingleton<PlayfieldSetting>.Instance.TimeAfterEnd,
				ITrack track => track.TimeEnd,
				StaticJudgeLine line => line.TimeMax,
				_ => T3Time.MaxValue
			};
		}
	}
}