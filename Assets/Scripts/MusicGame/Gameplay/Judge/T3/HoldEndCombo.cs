#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using UnityEngine.InputSystem.EnhancedTouch;

namespace MusicGame.Gameplay.Judge.T3
{
	public class HoldEndCombo : IComboItem
	{
		public ChartComponent FromComponent { get; set; }

		public T3Time ExpectedTime { get; set; }

		public HoldEndCombo(ChartComponent component) => FromComponent = component;

		public IJudgeItem GetNewJudgeItem() => new HoldEndJudgeItem(this);
	}

	public class HoldEndJudgeItem : IT3JudgeItem
	{
		IComboItem IJudgeItem.ComboItem => ComboItem;

		public HoldEndCombo ComboItem { get; }

		public T3Time ActualTime { get; set; }

		public float EndPosition { get; set; }

		public Touch? JudgedTouch { get; set; }

		public T3JudgeResult JudgeResult { get; set; }

		public HoldEndJudgeItem(HoldEndCombo combo) => ComboItem = combo;
	}
}