#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime;

namespace MusicGame.Models.Note.Combo
{
	public class DoubleCombo : IComboInfo
	{
		public T3Time TimeStart { get; private set; }
		public T3Time TimeEnd { get; private set; }

		private readonly T3Time[] times;

		public IEnumerable<T3Time> ComboTimes => times;

		public DoubleCombo(T3Time timeStart, T3Time timeEnd)
		{
			TimeStart = timeStart;
			TimeEnd = timeEnd;
			times = new[] { timeStart, timeEnd };
		}

		public DoubleCombo Set(T3Time timeStart, T3Time timeEnd)
		{
			TimeStart = timeStart;
			TimeEnd = timeEnd;
			times[0] = timeStart;
			times[1] = timeEnd;
			return this;
		}
	}
}