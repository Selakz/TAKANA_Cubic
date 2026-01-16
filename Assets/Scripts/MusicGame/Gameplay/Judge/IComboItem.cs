#nullable enable

using MusicGame.Gameplay.Chart;
using T3Framework.Runtime;

namespace MusicGame.Gameplay.Judge
{
	public interface IComboItem
	{
		public ChartComponent FromComponent { get; set; }

		public T3Time ExpectedTime { get; set; }

		public IJudgeItem GetNewJudgeItem();
	}
}