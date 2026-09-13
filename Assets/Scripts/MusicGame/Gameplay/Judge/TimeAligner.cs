#nullable enable

using MusicGame.Gameplay.Level;
using T3Framework.Runtime;
using T3Framework.Runtime.VContainer;
using T3Framework.Static;
using VContainer;

namespace MusicGame.Gameplay.Judge
{
	public class TimeAligner : HierarchySystem<TimeAligner>
	{
		// Private
		[Inject] private JudgeTimeAudioPlayer music = default!;

		public T3Time GetChartTime(double inputTime)
			=> music.GetChartTime(inputTime) + ISingleton<PlayfieldSetting>.Instance.InputDeviation;

		public T3Time GetCurrentChartTime()
			=> music.ChartTime + ISingleton<PlayfieldSetting>.Instance.InputDeviation;
	}
}