#nullable enable

using DLCore.Models;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;

namespace DLCore.Gameplay.Judge
{
	public class DLHoldEndCombo : IComboItem
	{
		public ChartComponent FromComponent { get; set; }

		public T3Time ExpectedTime { get; set; }

		public bool PlayHitSound { get; set; }

		public ColorVariant Color { get; set; }

		public DLHoldEndCombo(ChartComponent component) => FromComponent = component;

		public IJudgeItem GetNewJudgeItem() => new DLHoldEndJudgeItem(this);
	}

	public class DLHoldEndJudgeItem : IDLJudgeItem
	{
		IComboItem IJudgeItem.ComboItem => ComboItem;

		public DLHoldEndCombo ComboItem { get; }

		public T3Time ActualTime { get; set; }

		public T3JudgeResult JudgeResult { get; set; }

		public DLHoldEndJudgeItem(DLHoldEndCombo combo) => ComboItem = combo;
	}
}