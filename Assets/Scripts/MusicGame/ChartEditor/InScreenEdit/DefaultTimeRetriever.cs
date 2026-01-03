#nullable enable

using System;

namespace MusicGame.ChartEditor.InScreenEdit
{
	public class DefaultTimeRetriever : BaseTimeRetriever
	{
		public static DefaultTimeRetriever Instance { get; } =
			new Lazy<DefaultTimeRetriever>(() => new DefaultTimeRetriever()).Value;
	}
}