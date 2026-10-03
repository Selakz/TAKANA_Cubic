#nullable enable

using System.Linq;
using MusicGame.ChartEditor.TrackLayer;
using MusicGame.Models.JudgeLine;
using MusicGame.Models.Note;
using MusicGame.Models.Track;

namespace EditorPlugin.Tests.Cases
{
	/// <summary>
	/// C# side checks of the scenarios declared by
	/// <c>Assets/StreamingAssets/Excluded/EditorPluginTests/cases/chartClipboard.ts</c>.
	/// They assert the clipboard content the plugin left behind and that it never leaked into the chart.
	/// </summary>
	public static class ChartClipboardCase
	{
		[PluginTestCheck("chartClipboard.override-and-read")]
		public static void OverrideAndRead(PluginTestCheckContext ctx)
		{
			var item = ctx.ClipboardItems.SingleOrDefault();
			ctx.Check(item is not null, "exactly one clipboard item");
			if (item is not null)
			{
				ctx.Check(item.Component.Model is ITrack, "the item holds a track copy");
				ctx.Check(item.Component.BelongingChart is null, "the copy belongs to no chart");
				ctx.Check(item.Component.Parent is null, "the copy is a root component");
				ctx.Check(item.Parent is { Model: IJudgeLine }, "the item is attached under the judge line");
				ctx.Check(
					item.Component.Model is ITrack track &&
					track.GetLayerId() == ctx.Chart.GetsLayersInfo().DefaultLayer.Id,
					"the copy belongs to the default layer");
				var child = item.Component.Children.SingleOrDefault();
				ctx.Check(child is not null && child.Model is INote && child.Parent == item.Component,
					"the copied note is a child of the copied track");
			}

			ctx.Check(ctx.Tracks.Count() == 1, "the chart keeps its own track");
			ctx.Check(ctx.Notes.Count() == 1, "the chart keeps its own note");
			ctx.Check(ctx.Tracks.Single().Model.TimeMin.Milli == 0, "the chart's own track was not nudged");
			ctx.Check(ctx.Commands.BatchCount == 1, "the clipboard writes produced no command");
		}

		[PluginTestCheck("chartClipboard.note-copy-and-deleted-parent")]
		public static void NoteCopyAndDeletedParent(PluginTestCheckContext ctx)
		{
			var item = ctx.ClipboardItems.SingleOrDefault();
			ctx.Check(item is not null, "exactly one clipboard item");
			if (item is not null)
			{
				ctx.Check(item.Component.Model is Hit hit && hit.TimeJudge.Milli == 600,
					"the rejected override kept the previous content");
				ctx.Check(item.Component.BelongingChart is null, "the copy belongs to no chart");
				ctx.Check(item.Parent is not null && item.Parent.BelongingChart is null,
					"the parent is the track that left the chart");
			}

			ctx.Check(ctx.Tracks.Count() == 0, "the track left the chart");
			ctx.Check(ctx.Notes.Count() == 0, "the copied note never entered the chart");
			ctx.Check(ctx.Commands.BatchCount == 2, "the add and the removal produced two batches");
		}
	}
}
