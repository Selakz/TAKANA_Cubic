#nullable enable

using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime;

namespace MusicGame.Models.Note.Combo
{
	public class SingleCombo : IComboInfo
	{
		public T3Time TimeJudge { get; private set; }

		public IEnumerable<T3Time> ComboTimes => Enumerable.Repeat(TimeJudge, 1);

		public SingleCombo(T3Time timeJudge)
		{
			TimeJudge = timeJudge;
		}

		public SingleCombo Set(T3Time timeJudge)
		{
			TimeJudge = timeJudge;
			return this;
		}
	}
}