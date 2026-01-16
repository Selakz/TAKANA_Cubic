#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;
using UnityEngine.InputSystem.EnhancedTouch;

namespace MusicGame.Gameplay.Judge.T3
{
	public class HitCombo : IComboItem
	{
		public ChartComponent FromComponent { get; set; }

		public T3Time ExpectedTime { get; set; }

		public bool NeedTap { get; set; }

		public float LeftEdge { get; set; }

		public float RightEdge { get; set; }

		public HitCombo(ChartComponent component) => FromComponent = component;

		public IJudgeItem GetNewJudgeItem() => new HitJudgeItem(this);
	}

	public class HitJudgeItem : IT3JudgeItem
	{
		IComboItem IJudgeItem.ComboItem => ComboItem;

		public HitCombo ComboItem { get; set; }

		public T3Time ActualTime { get; set; }

		public float TapPosition { get; set; }

		public Touch? JudgedTouch { get; set; }

		public T3JudgeResult JudgeResult { get; set; }

		public HitJudgeItem(HitCombo combo) => ComboItem = combo;
	}
}