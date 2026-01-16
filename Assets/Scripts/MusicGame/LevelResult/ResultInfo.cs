#nullable enable

using System.Collections.Generic;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Level;

namespace MusicGame.LevelResult
{
	public class ResultInfo
	{
		public LevelInfo? LevelInfo { get; set; }

		public double Score { get; set; }

		public int Combo { get; set; }

		public int MaxCombo { get; set; }

		public IReadOnlyCollection<IComboItem>? ComboItems { get; set; }

		public IEnumerable<IJudgeItem>? JudgeItems { get; set; }
	}
}