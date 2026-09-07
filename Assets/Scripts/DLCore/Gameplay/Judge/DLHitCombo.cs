#nullable enable

using DLCore.Models;
using MusicGame.Gameplay.Chart;
using MusicGame.Gameplay.Judge;
using MusicGame.Gameplay.Judge.T3;
using T3Framework.Runtime;

namespace DLCore.Gameplay.Judge
{
	public class DLHitCombo : IComboItem
	{
		public ChartComponent FromComponent { get; set; }

		public T3Time ExpectedTime { get; set; }

		public bool NeedTap { get; set; }

		public bool PlayHitSound { get; set; }

		public ColorVariant Color { get; set; }

		public DLHitCombo(ChartComponent component) => FromComponent = component;

		public IJudgeItem GetNewJudgeItem() => new DLHitJudgeItem(this);
	}

	public class DLHitJudgeItem : IDLJudgeItem
	{
		IComboItem IJudgeItem.ComboItem => ComboItem;

		public DLHitCombo ComboItem { get; set; }

		public T3Time ActualTime { get; set; }

		public T3JudgeResult JudgeResult { get; set; }

		public DLHitJudgeItem(DLHitCombo combo) => ComboItem = combo;
	}
}