#nullable enable

using System.Linq;
using MusicGame.ChartEditor.TrackLayer;

namespace EditorPlugin.Tests.Cases
{
	/// <summary>
	/// C# side checks of the scenarios declared by
	/// <c>Assets/StreamingAssets/Excluded/EditorPluginTests/cases/chartBasics.ts</c>. The other scenarios of that file
	/// (bpm list, plugin params) are fully covered by the TypeScript assertions.
	/// </summary>
	public static class ChartBasicsCase
	{
		[PluginTestCheck("layers.edit-and-track-setLayer")]
		public static void LayersEditAndTrackSetLayer(PluginTestCheckContext ctx)
		{
			ctx.Check(ctx.Chart.GetsLayersInfo().Count == 1, "the added layer was removed again");
			ctx.Check(ctx.Tracks.Count() == 1, "track added on the new layer");
		}

		[PluginTestCheck("pluginBase.events")]
		public static void PluginBaseEvents(PluginTestCheckContext ctx)
		{
			ctx.Check(ctx.Tracks.Count() == 1, "track added by the event scenario");
			ctx.Check(ctx.Notes.Count() == 1, "note added by the event scenario");
		}
	}
}
