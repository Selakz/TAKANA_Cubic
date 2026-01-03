#nullable enable

using MusicGame.Models;
using T3Framework.Runtime;

namespace MusicGame.Gameplay.Stage
{
	public interface IModelTimeCalculator
	{
		public T3Time GetTimeInstantiate(IChartModel? model);

		public T3Time GetTimeDestroy(IChartModel? model);
	}
}