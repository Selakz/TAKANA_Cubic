#nullable enable

using System.Collections.Generic;
using System.Linq;
using T3Framework.Runtime;

namespace MusicGame.Models.Note.Combo
{
	public class EmptyCombo : IComboInfo
	{
		public static EmptyCombo Instance { get; } = new();

		public IEnumerable<T3Time> ComboTimes => Enumerable.Empty<T3Time>();
	}
}