#nullable enable

using System.Collections.Generic;
using T3Framework.Runtime;

namespace MusicGame.Models.Note.Combo
{
	public interface IComboInfo
	{
		/// <summary> Should be sorted in ascending order. </summary>
		public IEnumerable<T3Time> ComboTimes { get; }
	}
}