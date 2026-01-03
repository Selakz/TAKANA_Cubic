#nullable enable

using MusicGame.Models.JudgeLine;

namespace MusicGame.Gameplay.Chart
{
	public static class T3ChartExtensions
	{
		public static ChartComponent DefaultJudgeLine(this ChartInfo chart)
		{
			foreach (var component in chart)
			{
				var current = component;
				while (current is not null)
				{
					if (current.Model is IJudgeLine) return current;
					current = current.Parent;
				}
			}

			var newLine = chart.AddComponent(new StaticJudgeLine());
			return newLine;
		}
	}
}